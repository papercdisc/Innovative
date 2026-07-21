using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField] PlayerInputSubscription getInput;

    [SerializeField] float maxSpeed = 5f;
    [SerializeField] float currentSpeed = 0f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float deceleration = 10f;

    Vector2 playerMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (PlayerInputSubscription.Instance != null && getInput == null)
        {
            getInput = PlayerInputSubscription.Instance;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        playerMovement = new Vector2(getInput.MoveInput.x, getInput.MoveInput.y).normalized;

        if (playerMovement.magnitude > 0)
        {
            if (currentSpeed < maxSpeed)
                currentSpeed += acceleration * Time.fixedDeltaTime;
        }
        else
        {
            if (currentSpeed > 0)
                currentSpeed -= deceleration * Time.fixedDeltaTime;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);

        Vector2 newVelocity = playerMovement * currentSpeed;
        rb.linearVelocity = newVelocity;
    }
}
