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
    public bool JumpInput { get; private set; } = false;

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
    }

    private void OnDisable()
    {
        _Input.Actions.Move.performed -= SetMovement;
        _Input.Actions.Move.canceled -= SetMovement;

        _Input.Actions.Jump.performed -= SetJump;
        _Input.Actions.Jump.canceled -= SetJump;

        _Input.Actions.Look.performed -= SetLook;
        _Input.Actions.Look.canceled -= SetLook;

        _Input.Actions.Disable();
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
        if (ctx.control.device is Mouse)
        {
            LookInput = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        else
            LookInput = ctx.ReadValue<Vector2>();

        GetDeviceOnInput(ctx);
    }
    void SetJump(InputAction.CallbackContext ctx)
    {
        JumpInput = ctx.ReadValueAsButton();

        GetDeviceOnInput(ctx);
    }
}
