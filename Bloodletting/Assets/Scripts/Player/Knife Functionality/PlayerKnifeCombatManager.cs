using UnityEngine;
using UnityEngine.Events;

public class PlayerKnifeCombatManager : MonoBehaviour
{
    [Header("References")]
    public PlayerKnifeInventory Inventory { get; private set; }
    public PlayerKnifeAttackHandler AttackHandler { get; private set; }
    public PlayerKnifeRetrieveHandler RetrieveHandler { get; private set; }
    PlayerInputSubscription_FPS getInput;

    [field: Header("Player Preferences / Debug Settings")]
    [field: SerializeField, Tooltip("Switch between hold for ADS and a toggle. Hold for ADS is default.")]
    public bool toggleADSMode { get; private set; } = false;

    [field: Header("State Tracking")]
    [field: SerializeField] public HeldKnifeState KnifeState { get; private set; }
    //[field: SerializeField] public float atkCD { get; private set; } = 0.5f;
    [field: SerializeField] public InputBuffer atkBuffer { get; private set; } = new InputBuffer();
    [field: SerializeField] public InputBuffer retrieveBuffer { get; private set; } = new InputBuffer();

    [Header("Events")]
    public UnityEvent<bool> OnADS;

    // == PRIVATE VARIABLES ==
    float atkHoldTime = 0f;
    //bool canAttack = true; // if the player is allowed to attack (not on cooldown)

    private void Start()
    {
        Inventory = GetComponent<PlayerKnifeInventory>();
        AttackHandler = GetComponent<PlayerKnifeAttackHandler>();
        RetrieveHandler = GetComponent<PlayerKnifeRetrieveHandler>();
        getInput = PlayerInputSubscription_FPS.Instance;
    }

    private void Update()
    {
        ADSInputHandler();
        AttackInputHandler();
        RetrieveInputHandler();

        if(atkBuffer.IsActive && Inventory.HeldKnives > 0 && AttackHandler.CanAttack)
        {
            atkBuffer.TryConsume();
            AttackHandler.Attack(KnifeState);
        }
        if (retrieveBuffer.TryConsume())
        {
            RetrieveHandler.Retrieve();
        }
    }

    #region Input Handlers
    private void ADSInputHandler()
    {
        bool adsInputThisFrame = getInput.ADSPressedThisFrame;
        bool adsInputHeld = getInput.ADSHeld;

        // === PARSE ADS INPUT (depending on input preference) ===
        if (toggleADSMode) // handle inputs for toggle
        {
            if (adsInputThisFrame) { KnifeState = (KnifeState == HeldKnifeState.Aiming) ? HeldKnifeState.Melee : HeldKnifeState.Aiming; }
        }
        else // handle inputs for held ADS
        {
            if (adsInputHeld) { KnifeState = HeldKnifeState.Aiming; }
            else { KnifeState = HeldKnifeState.Melee; }
        }

        if (Inventory.HeldKnives > 0) OnADS?.Invoke(KnifeState == HeldKnifeState.Aiming);
    }
    private void AttackInputHandler()
    {
        // separate atk input buffer ticket & hold time tracking
        if (getInput.AttackPressedThisFrame) { atkHoldTime = 0f; } // reset on click
        else if (getInput.AttackHeld) { atkHoldTime += Time.deltaTime; } // increment while held
        else { atkHoldTime = 0f; } // reset if not held

        bool holdingPastTicket = getInput.AttackHeld && atkHoldTime > (atkBuffer.window); // only count as held if button is held past buffer ticket time

        if (getInput.AttackPressedThisFrame || holdingPastTicket)
        {
            atkBuffer.SetBuffer(); // reset buffer ticket
        }
        else if (atkBuffer.IsActive)
        {
            atkBuffer.Tick(Time.deltaTime); // decrement buffer ticket
        }
    }
    private void RetrieveInputHandler()
    {
        // recieve pickup input (less complex than attack cause holding does nothing different)
        if (getInput.RetrievePressedThisFrame)
        {
            retrieveBuffer.SetBuffer();
        }
        else if (retrieveBuffer.IsActive)
        {
            retrieveBuffer.Tick(Time.deltaTime);
        }
    }
    #endregion
}

[System.Serializable]
public class InputBuffer 
{
    // note: different from cooldown, this is a buffer that allows to queue an action
    // basically, every time the input is pressed, the buffer counter acts like an expiring ticket.
    // Once the "ticket" expires, the input is no longer valid, and cannot be consumed for a given action.
    // this allows for actions to be queued up if the cooldown is still active, but close to expiring.
    public float window = 0.2f;
    public float counter { get; private set; }

    public void SetBuffer() => counter = window; // set counter to window amount
    public void Tick(float dt) { if (counter > 0) counter -= dt; } // need to call externally in an update function, otherwise will not update
    public bool IsActive => counter > 0;
    public bool TryConsume() // if the counter hasn't expired, consider action executable.
    {
        if (counter <= 0) return false;
        counter = 0;
        return true; // bool is useful when calling on its own, but avoid doing so when there are other gates
    }
}
