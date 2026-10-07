using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    [SerializeField] GameObject pickupTrigger;
    public Vector3 moveDir;
    public float projSpeed = 10f;
    public float projDamage = 10f;
    Rigidbody rb;
    Vector3 velocity;
 
    KnifeProjState state = KnifeProjState.InFlight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        transform.rotation = Quaternion.LookRotation(moveDir);
        pickupTrigger.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if (state == KnifeProjState.InFlight)
        {
            velocity = moveDir.normalized * projSpeed;
            rb.linearVelocity = velocity;
        }
    }

    // Collision Behaviors
    public void PickupTrigger(Collider other)
    {
        if (state == KnifeProjState.CanPickup)
        {
            if (other.gameObject.GetComponent<PlayerCombatFP>())
            {
                other.gameObject.GetComponent<PlayerCombatFP>().AddKnife(1);
                Destroy(gameObject);
            }
        }
    }

    void DropKnife() 
    { 
        // remove from parent
        this.gameObject.transform.SetParent(null);
        rb.isKinematic = false;
        state = KnifeProjState.Dropped;
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

                if(enemyHealth.currentHealth >= 0)
                {
                    this.gameObject.transform.SetParent(collision.gameObject.transform);
                    enemyHealth.OnDeath.AddListener(() =>
                    {
                        DropKnife();
                    });
                }
                else DropKnife();
            }
            else
            {
                state = KnifeProjState.CanPickup;
                rb.isKinematic = true;

                pickupTrigger.SetActive(true);
            }
        }
        else if (state == KnifeProjState.Dropped)
        {
            if (collision.gameObject.GetComponentInParent<EnemyHealth>()) return; //ignore enemy collision

            state = KnifeProjState.CanPickup;
            rb.isKinematic = true;

            pickupTrigger.SetActive(true);
        }
    }
}
