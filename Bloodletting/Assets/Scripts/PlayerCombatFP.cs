using UnityEngine;

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

    [field: Header("Player Preferences")]
    [field: SerializeField, Tooltip("Switch between hold for ADS and a toggle. Hold for ADS is default.")] 
    public bool toggleADSMode { get; private set; } = false;

    [field: Header("Combat Settings")]
    // === COOLDOWN TRACKING ===
    [field: SerializeField] public float atkCD { get; private set; } = 0.5f;
    [field: SerializeField] public float atkBuffer { get; private set; } = 0.2f;
    
    float atkBufferCounter = 0; // note: different from cooldown, this is a buffer that allows to queue an attack
    bool canAttack = true; // if the player is allowed to attack (not on cooldown)

    // === KNIFE TRACKING ===
    [field: SerializeField] public int maxKnives {get; private set; } = 3 ;
    [field: SerializeField] public int heldKnives {get; private set; } = 0;
    [field: SerializeField] public KnifeState knifeState {get; private set; }

    private void Start()
    {
        getInput = PlayerInputSubscription_FPS.Instance;
    }

    private void Update()
    {
        KnifeInputHandler();
    }
    private void FixedUpdate()
    {
        TryMelee();
        TryThrow();
    }

    private void KnifeInputHandler()
    {
        bool adsInputThisFrame = getInput.ADSPressedThisFrame;
        bool adsInputHeld = getInput.ADSHeld;

        // === PARSE ADS INPUT (depending on input preference) ===
        if (toggleADSMode) // handle inputs for toggle
        {
            if (adsInputThisFrame) { knifeState = (knifeState == KnifeState.Aiming) ? KnifeState.Melee : KnifeState.Aiming; }
        }
        else // handle inputs for held ADS
        {
            if(adsInputHeld) { knifeState = KnifeState.Aiming; }
            else { knifeState = KnifeState.Melee; }
        }

        // === QUEUE ATTACK ===
        if (getInput.AttackPressedThisFrame) // if the player pressed the attack button this frame
        {
            if (knifeState == KnifeState.Aiming || knifeState == KnifeState.Melee)
            {
                atkBufferCounter = atkBuffer; // reset the attack buffer counter
            }
            else
            {
                Debug.Log("No knives equipped");
            }
        }
        else if (atkBufferCounter > 0) // attack button wasn't pressed this frame, but the buffer counter is still active
        {
            atkBufferCounter -= Time.deltaTime; // decrement the attack buffer counter
        }

        if(getInput.AttackHeld && canAttack) // if the player is holding the attack button and can attack, reset the buffer counter to allow for continuous attacks
        {
            atkBufferCounter = atkBuffer;
        }
    }

    private void TryThrow()
    {
        if (heldKnives == 0) { return; }
        if (knifeState != KnifeState.Aiming) { return; }
        if (atkBufferCounter <= 0 || !canAttack) { return; }

        canAttack = false;
        atkBufferCounter = 0;

        Debug.Log("Throwing knife");

        // assuming all conditions are met, throw the knife
        // 1. Instantiate the knife prefab at the spawn point
        // 2. Set velocity of the knife to the player's orientation (child object) * throw force
        // 3. Decrease heldKnives and store the thrown knife in a list for potential pickup later

        Invoke(nameof(ResetAtk), atkCD); // reset attack after cooldown)
    }
    private void TryMelee()
    {
        if (heldKnives == 0) { return; }
        if (knifeState != KnifeState.Melee) { return; }
        if (atkBufferCounter <= 0 || !canAttack) { return; }

        canAttack = false;
        atkBufferCounter = 0;

        Debug.Log("Melee attack");

        // assuming all conditions are met, perform melee attack
        // 1. Play melee animation
        // 2. Check for enemies in range and apply damage

        Invoke(nameof(ResetAtk), atkCD); // reset attack after cooldown)
    }

    void ResetAtk()
    {
        canAttack = true;
    }
}
