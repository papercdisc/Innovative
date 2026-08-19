using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSubscription_FPS : MonoBehaviour
{
    public static PlayerInputSubscription_FPS Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        enabled = true;
    }
    public Vector2 MoveInput { get; private set; } = Vector2.zero;
    public Vector2 LookInput { get; private set; } = Vector2.zero;

    // Jump input
    public bool JumpPressedThisFrame { get; private set; } = false;
    public bool JumpHeld { get; private set; } = false;

    // ADS = Aim Down Sights
    public bool ADSPressedThisFrame { get; private set; } = false;
    public bool ADSHeld { get; private set; } = false;

    // Attack input
    public bool AttackPressedThisFrame { get; private set; } = false;
    public bool AttackHeld { get; private set; } = false;

    Player_FirstPerson _Input = null;

    public InputDevice currentDevice { get; private set; } = null;

    private void OnEnable()
    {
        _Input = new Player_FirstPerson();
        _Input.Actions.Enable();

        _Input.Actions.Move.performed += SetMovement;
        _Input.Actions.Move.canceled += SetMovement;

        _Input.Actions.Jump.performed += SetJump;
        _Input.Actions.Jump.canceled += SetJump;

        _Input.Actions.Look.performed += SetLook;
        _Input.Actions.Look.canceled += SetLook;

        _Input.Actions.ADS.performed += SetADS;
        _Input.Actions.ADS.canceled += SetADS;

        _Input.Actions.Attack.performed += SetAttack;  
        _Input.Actions.Attack.canceled += SetAttack;
    }

    private void OnDisable()
    {
        _Input.Actions.Move.performed -= SetMovement;
        _Input.Actions.Move.canceled -= SetMovement;

        _Input.Actions.Jump.performed -= SetJump;
        _Input.Actions.Jump.canceled -= SetJump;

        _Input.Actions.Look.performed -= SetLook;
        _Input.Actions.Look.canceled -= SetLook;

        _Input.Actions.ADS.performed -= SetADS;
        _Input.Actions.ADS.canceled -= SetADS;

        _Input.Actions.Attack.performed -= SetAttack;
        _Input.Actions.Attack.canceled -= SetAttack;

        _Input.Actions.Disable();
    }

    private void LateUpdate()
    {
        if(JumpPressedThisFrame == true)
        {
            JumpPressedThisFrame = false; // reset jump input after it has been read by the player movement script, so that it doesn't keep jumping every frame
        }
    }

    void GetDeviceOnInput(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device != currentDevice)
        {
            currentDevice = ctx.control.device;
        }
    }

    void SetMovement(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();

        GetDeviceOnInput(ctx);
    }
    void SetLook(InputAction.CallbackContext ctx)
    {
        LookInput = ctx.ReadValue<Vector2>();

        GetDeviceOnInput(ctx);
    }
    void SetJump(InputAction.CallbackContext ctx)
    {
        JumpHeld = ctx.ReadValueAsButton();

        if (ctx.performed)
        JumpPressedThisFrame = ctx.ReadValueAsButton();

        GetDeviceOnInput(ctx);
    }
    void SetADS(InputAction.CallbackContext ctx)
    {
        ADSHeld = ctx.ReadValueAsButton();
        if (ctx.performed)
        {
            ADSPressedThisFrame = ctx.ReadValueAsButton();
        }
        GetDeviceOnInput(ctx);
    }
    void SetAttack(InputAction.CallbackContext ctx)
    {
        AttackHeld = ctx.ReadValueAsButton();
        if (ctx.performed)
        {
            AttackPressedThisFrame = ctx.ReadValueAsButton();
        }
        GetDeviceOnInput(ctx);
    }
}
