using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;

/// <summary>
/// Knife throwing mechanic:
/// - throw knife while aiming (RMB) and pressing LMB
/// - have a few knives in inventory (can pick up more, up to maximum)
/// - knife is discarded after throwing, and can be picked up again
/// - knife is automatically picked up upon a headshot kill
/// </summary>
public class PlayerCombatFP : MonoBehaviour
{
    PlayerInputSubscription_FPS getInput;
    public static PlayerCombatFP Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    [Header("References")]
    [SerializeField] private Transform knifeSpawnPoint;
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private Transform orientation;
    
    // ~~~

    [field: Header("Player Preferences")]
    [field: SerializeField, Tooltip("Switch between hold for ADS and a toggle. Hold for ADS is default.")] 
    public bool toggleADSMode { get; private set; } = false;

    // ~~~

    [field: Header("Combat Settings")]
    [field: SerializeField] public float atkCD { get; private set; } = 0.5f;
    [field: SerializeField, Tooltip("The amount of time an attack input is buffered before it expires.")] public float atkBuffer { get; private set; } = 0.2f;
    
    float atkBufferCounter = 0; // note: different from cooldown, this is a buffer that allows to queue an attack
                                // basically, every time the input is pressed, the buffer counter acts like an expiring ticket.
                                // Once the "ticket" expires, the input is no longer valid, and cannot be consumed for an attack.
                                // this allows for attacks to be queued up if the cooldown is still active, but close to expiring.
                                // (explaining it here before i forget cause its a little confusing)
    float atkHoldTime = 0f;
    bool canAttack = true; // if the player is allowed to attack (not on cooldown)

    // ~~~

    [Header("Knife Retrieval")]
    [SerializeField] private float retrieveRange = 25f;
    [SerializeField] private float finisherDamage = 60f;
    [SerializeField] private float retrieveFinisherRange = 5f; // range for unique enemy finisher retrieval 
    [SerializeField, Range(1f, 30f), Tooltip("Half-angle of FOV cone for retrieving knives.")] private float retrieveAngle = 8f;
    [SerializeField] private LayerMask obstructionMask;
    float retrieveBuffer = 0.2f;
    float retrieveBufferCounter = 0f;

    // ~~~

    [Header("Knife Tracking")]
    [field: SerializeField] public int maxKnives {get; private set; } = 1 ;
    [field: SerializeField] public int heldKnives {get; private set; } = 0;
    [field: SerializeField] public HeldKnifeState knifeState {get; private set; }
    [field: SerializeField] public float maxKnifeLifetime { get; private set; } = 10;
    [field: SerializeField] public List<KnifeProjectile> SpawnedKnives { get; private set; } = new List<KnifeProjectile>();

    // ~~~

    [Header("Events")]
    public UnityEvent<int> OnKnifeCountChanged;
    public UnityEvent<bool> OnADS;
    public UnityEvent OnKnifeThrown;

    private void Start()
    {
        getInput = PlayerInputSubscription_FPS.Instance;
    }

    private void Update()
    {
        ADSInputHandler();
        AttackInputHandler();
        RetrieveInputHandler();

        TryRetrieve();
    }
    private void FixedUpdate()
    {
        TryAttack();
        //TryRetrieve();
    }

    private void ADSInputHandler()
    {
        bool adsInputThisFrame = getInput.ADSPressedThisFrame;
        bool adsInputHeld = getInput.ADSHeld;

        // === PARSE ADS INPUT (depending on input preference) ===
        if (toggleADSMode) // handle inputs for toggle
        {
            if (adsInputThisFrame) { knifeState = (knifeState == HeldKnifeState.Aiming) ? HeldKnifeState.Melee : HeldKnifeState.Aiming; }
        }
        else // handle inputs for held ADS
        {
            if(adsInputHeld) { knifeState = HeldKnifeState.Aiming;}
            else { knifeState = HeldKnifeState.Melee;}
        }

        if(heldKnives > 0) OnADS?.Invoke(knifeState == HeldKnifeState.Aiming);
    }
    private void AttackInputHandler()
    {
        // separate atk input buffer ticket & hold time tracking
        if(getInput.AttackPressedThisFrame) { atkHoldTime = 0f; } // reset on click
        else if (getInput.AttackHeld) { atkHoldTime += Time.deltaTime; } // increment while held
        else { atkHoldTime = 0f; } // reset if not held

        bool holdingPastTicket = getInput.AttackHeld && atkHoldTime > (atkBuffer); // only count as held if button is held past buffer ticket time

        if (getInput.AttackPressedThisFrame || holdingPastTicket)
        {
            atkBufferCounter = atkBuffer; // reset buffer ticket
        }
        else if (atkBufferCounter > 0)
        {
            atkBufferCounter -= Time.deltaTime; // decrement buffer ticket
        }
    }
    private void RetrieveInputHandler()
    {
        // recieve pickup input (less complex than attack cause holding does nothing different)
        if(getInput.RetrievePressedThisFrame)
        {
            retrieveBufferCounter = retrieveBuffer;
        }
        else if (retrieveBufferCounter > 0)
        {
            retrieveBufferCounter -= Time.deltaTime;
        }
    }

    private void TryAttack() // actually execute attack if conditions are met (called in FixedUpdate to avoid frame rate issues)
    {
        if (heldKnives <= 0) { return; }
        if (atkBufferCounter <= 0 || !canAttack) { return; }

        canAttack = false;
        atkBufferCounter = 0;

        if (knifeState == HeldKnifeState.Aiming)
        { 
            // assuming all conditions are met, throw the knife
            // 1. Instantiate the knife prefab at the spawn point
            // 2. Set velocity of the knife to the player's orientation (child object) * throw force
            // 3. Decrease heldKnives and store the thrown knife in a list for potential pickup later

            OnKnifeThrown?.Invoke(); // invoke event to trigger HUD animation
            heldKnives--; // decrease held knives
            OnKnifeCountChanged?.Invoke(heldKnives); // invoke event to update UI

            // find trajectory
            Ray shootDir = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // shoot from center of screen

            KnifeProjectile knife = Instantiate(knifePrefab, knifeSpawnPoint.position, Quaternion.identity).GetComponent<KnifeProjectile>();
            //Debug.Log($"Assigning lifetime: {maxKnifeLifetime}");
            knife.lifetime = maxKnifeLifetime;
            knife.moveDir = shootDir.direction;

            SpawnedKnives.Add(knife);

            //Debug.Log("Throwing knife"); 
        }
        else if (knifeState == HeldKnifeState.Melee)
        { 
            // assuming all conditions are met, perform melee attack
            // 1. Play melee animation
            // 2. Check for enemies in range and apply damage

            //Debug.Log("Melee attack"); 
        }

        Invoke(nameof(ResetAtk), atkCD); // reset attack after cooldown)
    }
    private void TryRetrieve()
    {
        if (retrieveBufferCounter <= 0) return;

        retrieveBufferCounter = 0;

        // input only goes through if the player is aiming at/near a knife
        KnifeProjectile knifeToRetrieve = FindKnifeInView();
        if (knifeToRetrieve == null) return;

        // depending on proximity to knife + if its in an enemy, do different things
        Vector3 toKnife = knifeToRetrieve.transform.position - Camera.main.transform.position;
        float dist = toKnife.magnitude;

        // if in an enemy + close to knife, perform special attack (+bonus damage) and retrieve knife
        if(dist < retrieveFinisherRange)
        {
            if(knifeToRetrieve.GetComponentInParent<EnemyHealth>() != null)
            {
                EnemyHealth enemy = knifeToRetrieve.GetComponentInParent<EnemyHealth>();

                enemy.TakeDamage(finisherDamage);
            }
        }

        // if in an enemy and/or far from knife, pull knife back towards player (like a boomerang) and retrieve knife (deal damage during flight, but dont stick)
        // * damage in this instance is lessened

        knifeToRetrieve.ReturnToPlayer();
    }

    private KnifeProjectile FindKnifeInView()
    {
        Transform cam = Camera.main.transform;
        float minDot = Mathf.Cos(retrieveAngle * Mathf.Deg2Rad); // minimum COS for knives to be considered within view

        KnifeProjectile bestMatch = null;
        float bestDot = minDot;

        foreach (KnifeProjectile knife in SpawnedKnives)
        {
            if (knife == null) continue;
            if (knife.State != KnifeProjState.CanPickup) continue;

            Vector3 toKnife = knife.transform.position - cam.position; // direction + mag to knife
            float dist = toKnife.magnitude; // distance as float
            if (dist > retrieveRange) continue; // ignore if outside of (distance) range

            float dot = Vector3.Dot(cam.forward, toKnife.normalized);
            if (dot < bestDot) continue; // ignore if outside cone, or worse than current best

            // check if in LOS (allow hitting the knife itself, since it may be stuck in a wall)
            if (Physics.Linecast(cam.position, knife.transform.position, out RaycastHit hit, obstructionMask)
                && hit.transform != knife.transform) continue;

            bestMatch = knife;
            bestDot = dot;
        }

        Debug.Log("Best Match is: " + bestMatch);
        return bestMatch;
    }

    void ResetAtk()
    {
        canAttack = true;
    }
    public void AddKnife(int amount)
    {
        heldKnives = Mathf.Clamp(heldKnives + amount, 0, maxKnives);
        OnKnifeCountChanged?.Invoke(heldKnives); // invoke event to update UI
    }
}
