using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    Rigidbody rb;
    Vector3 velocity;

    [Header("Properties")]
    public Vector3 moveDir;
    public float projSpeed = 10f;
    public float projDamage = 10f;
    public float lifetime = 10; // determined by PlayerCombatFP

    [Header("Arc")]
    [SerializeField, Range(0, 1.5f), Tooltip("The higher the value, the more pronounced the arc")] 
    float gravityScale = 0.5f; 
    [SerializeField, Tooltip("Adds vertical tilt to launch direction (compensates for arc)")] 
    float upwardBias = 0.1f;

    KnifeProjState state = KnifeProjState.InFlight;
    public KnifeProjState State => state;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // custom gravity scaling is used

        Vector3 launchDir = (moveDir + Vector3.up * upwardBias).normalized;
        velocity = launchDir * projSpeed;
        rb.linearVelocity = velocity;

        transform.rotation = Quaternion.LookRotation(moveDir);
    }

    // Update is called once per frame
    void Update()
    {
        if(state == KnifeProjState.CanPickup)
        {
            lifetime -= Time.deltaTime;

            if(lifetime <= 0)
            {
                ReturnToPlayer();
            }
        }
    }
    private void FixedUpdate()
    {
        if (state == KnifeProjState.InFlight)
        {
            velocity += Physics.gravity * gravityScale * Time.fixedDeltaTime;
            rb.linearVelocity = velocity;

            // keep the blade pointing along its path so it noses down as it falls
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }

    public void ReturnToPlayer()
    {
        //Debug.Log($"ReturnToPlayer: state={state}, lifetime={lifetime}\n{System.Environment.StackTrace}", this);

        PlayerCombatFP.Instance.AddKnife(1);

        if (PlayerCombatFP.Instance.SpawnedKnives.Contains(this))
        {
            PlayerCombatFP.Instance.SpawnedKnives.Remove(this);
        }

        Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state == KnifeProjState.InFlight)
        {
            // add enemy related logic later
            if (collision.gameObject.GetComponent<PlayerHealth>()) return; //ignore player collision

            if(collision.gameObject.GetComponentInParent<EnemyHealth>())
            {
                EnemyHealth enemyHealth = collision.gameObject.GetComponentInParent<EnemyHealth>();
                enemyHealth.TakeDamage(projDamage);
                state = KnifeProjState.InEnemy;
                rb.isKinematic = true;

                if (!enemyHealth.isDead)
                {
                    this.gameObject.transform.SetParent(collision.gameObject.transform);
                    enemyHealth.OnDeath.AddListener(() =>
                    {
                        ReturnToPlayer();
                    });
                }
                else ReturnToPlayer();

                return;
            }
            else
            {
                state = KnifeProjState.CanPickup;
                rb.isKinematic = true;
            }

        }

    }
}
