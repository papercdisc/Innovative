using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    Rigidbody rb;
    Collider col;
    Vector3 velocity;

    [Header("Properties")]
    public Vector3 moveDir;
    public float projSpeed = 10f;
    public float retrieveSpeed = 20f;
    public float projDamage = 10f;
    public float lifetime = 10; // determined by PlayerCombatFP

    [Header("Arc")]
    [SerializeField, Range(0, 1.5f), Tooltip("The higher the value, the more pronounced the arc")] 
    float gravityScale = 0.5f; 
    [SerializeField, Tooltip("Adds vertical tilt to launch direction (compensates for arc)")] 
    float upwardBias = 0.1f;

    // HashSet is like a list but cannot have duplicate items + items don't have an index
    // its also faster at pulling up items than .Contains is
    // useful in this instance because we just need to know if enemies have been hit or not
    HashSet<EnemyHealth> enemiesHitOnRetrieve = new HashSet<EnemyHealth>();

    KnifeProjState state = KnifeProjState.InFlight;
    public KnifeProjState State => state;

    EnemyHealth stuckEnemy;
    Transform returnTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        col = GetComponent<Collider>();
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
                QuickReturnToPlayer();
            }
        }
    }
    private void FixedUpdate()
    {
        if (state == KnifeProjState.InFlight) // arc trajectory
        {
            velocity += Physics.gravity * gravityScale * Time.fixedDeltaTime;
            rb.linearVelocity = velocity;

            // keep the blade pointing along its path so it noses down as it falls
            transform.rotation = Quaternion.LookRotation(velocity);
        }
        if(state == KnifeProjState.Retrieving) // straight 
        {
            Vector3 toTarget = returnTarget.position - transform.position;
            rb.linearVelocity = toTarget.normalized * retrieveSpeed; // move in direction to target

            if (rb.linearVelocity.sqrMagnitude > 0.01f) // if moving, face move direction
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    public void QuickReturnToPlayer()
    {
        //Debug.Log($"ReturnToPlayer: state={state}, lifetime={lifetime}\n{System.Environment.StackTrace}", this);

        PlayerCombatFP.Instance.AddKnife(1);

        if (PlayerCombatFP.Instance.SpawnedKnives.Contains(this))
        {
            PlayerCombatFP.Instance.SpawnedKnives.Remove(this);
        }

        Destroy(this.gameObject);
    }
    public void SetRetrieving(Transform target)
    {
        state = KnifeProjState.Retrieving;
        returnTarget = target;

        if (stuckEnemy != null) UnstickEnemy();

        rb.isKinematic = false;
        col.isTrigger = true; // when retrieving, we don't want collision.
    }

    private void OnCollisionEnter(Collision collision) // could probably reformat this thing to be less redundant but I don't feel like it rn
    {
        if (state == KnifeProjState.InFlight)
        {
            if (collision.gameObject.GetComponent<PlayerHealth>()) return; //ignore player collision

            EnemyHealth enemyHealth = collision.gameObject.GetComponentInParent<EnemyHealth>();
            if(enemyHealth)
            {
                StickToEnemy(enemyHealth, collision);

                return;
            }
            else
            {
                state = KnifeProjState.CanPickup;
                rb.isKinematic = true;
            }

        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (state != KnifeProjState.Retrieving) return;

        if (other.gameObject.GetComponent<PlayerHealth>())
        {
            QuickReturnToPlayer();
            return; //ignore player collision
        }

        EnemyHealth enemy = other.gameObject.GetComponentInParent<EnemyHealth>();
        if (enemy) // equivalent to (enemy != null)
        {
            PierceEnemy(enemy, 0.75f);

            return;
        }
    }
    void UnstickEnemy()
    {
        this.gameObject.transform.SetParent(null);
        stuckEnemy.OnDeath.RemoveListener(QuickReturnToPlayer);
    }
    void StickToEnemy(EnemyHealth enemyHealth, Collision collision)
    {
        enemyHealth.TakeDamage(projDamage); // self explanatory
        state = KnifeProjState.InEnemy;
        rb.isKinematic = true; // stop moving

        if (!enemyHealth.isDead) // if, after doing damage, enemy is still alive, subscribe to OnDeath event 
        {
            this.gameObject.transform.SetParent(collision.gameObject.transform);
            stuckEnemy = enemyHealth;
            stuckEnemy.OnDeath.AddListener(QuickReturnToPlayer);
        }
        else QuickReturnToPlayer();
    }
    void PierceEnemy(EnemyHealth enemy, float dmgMulti = 1)
    {
        if (!enemiesHitOnRetrieve.Add(enemy)) return;

        enemy.TakeDamage(projDamage * dmgMulti);

        //foreach (Collider c in enemy.GetComponentsInChildren<Collider>())
        //    Physics.IgnoreCollision(col, c);
    }
}
