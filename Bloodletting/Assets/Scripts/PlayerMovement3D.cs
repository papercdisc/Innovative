using UnityEngine;

public class PlayerMovement3D : MonoBehaviour
{
    public static PlayerMovement3D Instance { get; private set; }
    void Awake()
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

    // References ~~~
    Rigidbody rb;
    [SerializeField] Transform orientation; // NOT parent object, but the child object. serves as a reference point for movement direction
    PlayerInputSubscription_FPS getInput;
    // ~~~~~~~~~~~~~~

    [SerializeField] LayerMask mask;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    Vector3 moveDir;
    float yRot; // to be used for rotation updates from the camera script down the line

    // Jumping and Falling Mechanics ~~~
    bool isGrounded = true; // if player is on ground
    bool canJump = true; // if jump is not on cooldown
    
    float jumpCD;
    float jumpBuffer = 0.2f; // time window to queue jump input before landing
    float jumpBufferCounter = 0;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 5f;

    [Tooltip("Affects gravity during the jump ascent. 1 acts as the baseline, values above 1 increase gravity, values below 1 decrease gravity.")]
    [SerializeField] AnimationCurve gravityRise;
    [Tooltip("Affects gravity during the jump descent. 1 acts as the baseline, values above 1 increase gravity, values below 1 decrease gravity.")]
    [SerializeField] AnimationCurve gravityFall;

    [SerializeField] float maxFallSpeed;
    float launchVel;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        getInput = PlayerInputSubscription_FPS.Instance;
    }

    private void Update()
    {
        CheckGrounded();
        JumpHandler();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        ExecuteJump();
        HandleJumpGravity();
    }

    private void MovePlayer()
    {
        moveDir = orientation.forward * getInput.MoveInput.y + orientation.right * getInput.MoveInput.x; // calculate direction based on fwd+back and left+right input

        Vector3 newVel = moveDir.normalized * moveSpeed; // calculate new velocity based on direction and speed

        rb.linearVelocity = newVel + new Vector3(0, rb.linearVelocity.y, 0); // maintain the current y velocity (gravity)
    }

    private void JumpHandler() // handle inputs and cooldown
    {
        // === INPUT HANDLING ===
        bool wantsToJump = getInput.JumpPressedThisFrame || getInput.JumpHeld;


        if (wantsToJump) // if jump input is pressed
        {
            jumpBufferCounter = jumpBuffer; // reset jump buffer counter
        }
        else if(jumpBufferCounter > 0) // if jump input is not pressed but buffer is still active
        {
            jumpBufferCounter -= Time.deltaTime; // decrement buffer counter
        }
    }

    private void ExecuteJump() // execute jump if conditions are met
    {
        if (jumpBufferCounter <= 0 || !isGrounded || !canJump) return;

        // ~~~ reset flags ~~~
        jumpBufferCounter = 0; // consume jump
        canJump = false;

        // ~~~ apply jump force ~~~
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset y velocity to 0 before applying jump force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        launchVel = rb.linearVelocity.y; // store the launch velocity for gravity calculations

        // === COOLDOWN HANDLING ===
        Invoke(nameof(ResetJump), jumpCD); // reset jump availability after cooldown
    }

    private void HandleJumpGravity()
    {
        if (isGrounded) return;

        float velY = rb.linearVelocity.y; // current y velocity

        if (velY > 0) // rising
        {
            float t = launchVel > 0 ? velY / launchVel : 0; // normalize the current y velocity to a value between 0 and 1 based on the launch velocity
            float multiplier = gravityRise.Evaluate(t); // evaluate the gravity multiplier based on the normalized value
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplier - 1) * Time.fixedDeltaTime; // apply the gravity multiplier to the current y velocity
        }
        else if (velY < 0) // falling
        {
            float t = Mathf.Clamp01(-velY / maxFallSpeed); // normalize the current y velocity to a value between 0 and 1 based on the max fall speed
                                                           // put simply, give the proper range of 0-1 for the gravity curve to evaluate
                                                           // the gravity multiplier to be applied is based on this current normalized y velocity
            float multiplier = gravityFall.Evaluate(t); // evaluate the gravity multiplier based on the normalized value
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplier - 1) * Time.fixedDeltaTime; // apply the gravity multiplier to the current y velocity
        }
    }

    void CheckGrounded()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f; // slightly above the player's position just in case
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, 0.3f, mask);
    }
    void ResetJump()
    {
        canJump = true;
    }

    public void SetYaw(float newYRot) => yRot = newYRot; // update the y rotation based on camera rotation
}
