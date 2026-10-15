using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    [SerializeField] GameObject pickupTrigger;
    public Vector3 moveDir;
    public float projSpeed = 10f;
    public float projDamage = 10f;
    public float lifetime = 10;
    Rigidbody rb;
    Vector3 velocity;
 
    KnifeProjState state = KnifeProjState.InFlight;
    public KnifeProjState State => state;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        transform.rotation = Quaternion.LookRotation(moveDir);

        //if(pickupTrigger != null)
        //    pickupTrigger.SetActive(false);
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
            velocity = moveDir.normalized * projSpeed;
            rb.linearVelocity = velocity;
        }
    }

    // Collision Behaviors
    //public void PickupTrigger(Collider other)
    //{
    //    if (state == KnifeProjState.CanPickup)
    //    {
    //        if (other.gameObject.GetComponent<PlayerCombatFP>())
    //        {
    //            other.gameObject.GetComponent<PlayerCombatFP>().AddKnife(1);
    //            Destroy(gameObject);
    //        }
    //    }
    //}

    //public void DropKnife() 
    //{ 
    //    // remove from parent
    //    this.gameObject.transform.SetParent(null);
    //    rb.isKinematic = false;
    //    state = KnifeProjState.Dropped;
    //}

    public void ReturnToPlayer()
    {
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

                if (enemyHealth.currentHealth >= 0)
                {
                    this.gameObject.transform.SetParent(collision.gameObject.transform);
                    enemyHealth.OnDeath.AddListener(() =>
                    {
                        //DropKnife();
                        ReturnToPlayer();
                    });
                }
                else ReturnToPlayer(); //DropKnife();

                return;
            }
            else
            {
                //if (this.gameObject.transform.position.y > PlayerMovement3D.Instance.transform.position.y + 2)
                //{
                //    DropKnife();

                //    return;
                //}

                state = KnifeProjState.CanPickup;
                rb.isKinematic = true;

                //if (pickupTrigger != null) pickupTrigger.SetActive(true);
            }

        }
        //else if (state == KnifeProjState.Dropped)
        //{
        //    if (collision.gameObject.GetComponentInParent<EnemyHealth>()) return; //ignore enemy collision

        //    state = KnifeProjState.CanPickup;
        //    rb.isKinematic = true;

        //    pickupTrigger.SetActive(true);
        //}
    }
}
