using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    // References
    EnemyHealth enemyHealth;
    Rigidbody2D rb;

    [SerializeField] EnemyType enemyType;
    [SerializeField] EnemyState behaviorState;

    [SerializeField] float moveSpeed;
    [SerializeField] float aggroRadius;
    [SerializeField] float avoidanceDist = 4;

    [SerializeField] float damageAmount = 10f;
    [SerializeField] float damageInterval = 1f;
    Coroutine damageOverTimeCoroutine;

    float timeSinceLastDamage = 0f;

    [SerializeField] LayerMask mask;

    GameObject target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayerHealth.Instance != null)
        {
            target = PlayerHealth.Instance.gameObject;
        }
        enemyHealth = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (enemyType) { 
            case EnemyType.Chaser:
                    ChaserUpdate();
                    break;
            case EnemyType.Coward:
                    CowardUpdate();
                    break;
            case EnemyType.Saboteur:
                    SaboteurUpdate();
                    break;
        }

        timeSinceLastDamage += Time.deltaTime;
    }

    void ChaserUpdate()
    {
        // move towards player if in aggro range
        if (Vector2.Distance(transform.position, target.transform.position) <= aggroRadius)
        {
            behaviorState = EnemyState.Aggro;
        }
        else
        {
            behaviorState = EnemyState.Idle;
        }
    }
    void CowardUpdate()
    {
        // kind of like the chaser, but runs away after hitting the player or if its health is low

        if (Vector2.Distance(transform.position, target.transform.position) <= aggroRadius)
        {
            if(enemyHealth.currentHealth <= enemyHealth.maxHealth / 2)
            {
                behaviorState = EnemyState.Flee;
            }
            else
            {
                behaviorState = EnemyState.Aggro;
            }
        }
        else
        {
            behaviorState = EnemyState.Idle;
        }
    }
    void SaboteurUpdate()
    {
        // runs towards player, but keeps distance if the player has high health/overhealth
        if (Vector2.Distance(transform.position, target.transform.position) <= aggroRadius)
        {
            if(PlayerHealth.Instance != null)
            {
                if (PlayerHealth.Instance.currentHealth >= PlayerHealth.Instance.maxHealth * 0.8f)
                {
                    behaviorState = EnemyState.Flee;
                }

                else
                {
                    behaviorState = EnemyState.Aggro;
                }
            }
        }
        else
        {
            behaviorState = EnemyState.Idle;
        }
    }

    private void FixedUpdate()
    {
        if (behaviorState == EnemyState.Aggro)
        {
            RunToTarget();
        }
        else if (behaviorState == EnemyState.Flee)
        {
            RunFromTarget();
        }

        if (AvoidObstacles() != rb.linearVelocity.normalized)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, AvoidObstacles(), Time.deltaTime * 10);

            Debug.DrawRay(transform.position, AvoidObstacles() * avoidanceDist, Color.red);
        }
    }

    private void RunToTarget()
    {
        // move towards player
        Vector2 direction = (target.transform.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }
    private void RunFromTarget()
    {
        Vector2 direction = (transform.position - target.transform.position).normalized;

        rb.linearVelocity = direction * (moveSpeed);
    }

    Vector2 AvoidObstacles() // doesnt work :(
    {
        // first, need to consider original pathing target (using weights)
        // if there is an obstacle in the way, steer away from it
        // 1) raycast in the direction of movement
        // 2) if the raycast hits an obstacle, send out two more raycasts at angles to the left and right of the original raycast
        // 3) steer in the direction of the ray with the highest magnitude (if both are the same, pick one at random)
        // 3.a) if the chosen raycast hits an obstacle, reverse its direction.

        RaycastHit2D hit = Physics2D.Raycast(transform.position, rb.linearVelocity.normalized, avoidanceDist, mask);
        if (hit.collider != null)
        {
            //Vector2 fwdLeft = Quaternion.Euler(0, 0, 45) * rb.linearVelocity.normalized;
            //Vector2 fwdRight = Quaternion.Euler(0, 0, -45) * rb.linearVelocity.normalized;

            Vector2 trueLeft = Quaternion.Euler(0, 0, 90) * rb.linearVelocity.normalized;
            Vector2 trueRight = Quaternion.Euler(0, 0, -90) * rb.linearVelocity.normalized;

            Vector2 bckLeft = Quaternion.Euler(0, 0, 135) * rb.linearVelocity.normalized;
            Vector2 bckRight = Quaternion.Euler(0, 0, -135) * rb.linearVelocity.normalized;

            RaycastHit2D leftHit = Physics2D.Raycast(transform.position, trueLeft, avoidanceDist, mask);
            RaycastHit2D rightHit = Physics2D.Raycast(transform.position, trueRight, avoidanceDist, mask);

            Vector2 newDir = Vector2.zero;

            if (leftHit.collider == null && rightHit.collider == null)
            {
                // both paths are clear, pick one at random
                if (Random.value < 0.5f)
                {
                    newDir = trueLeft;
                }
                else
                {
                    newDir = trueRight;
                }
            }
            else if (leftHit.collider == null)
            {
                newDir = trueLeft;
            }
            else if (rightHit.collider == null)
            {
                newDir = trueRight;
            }
            else
            {
                // check which magnitude is higher
                if (leftHit.distance > rightHit.distance)
                {
                    newDir = bckLeft;
                }
                else
                {
                    newDir = bckRight;
                }
            }
            return newDir.normalized;
        }
        else
        {
            return rb.linearVelocity.normalized;
        }
    }

    IEnumerator DamagePlayerOverTime(Collision2D collision)
    {
        while (collision.gameObject.GetComponent<PlayerHealth>() != null)
        { 
            timeSinceLastDamage = 0f;
            
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageAmount);

            yield return new WaitForSeconds(damageInterval); // Adjust the time interval as needed
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerHealth>() != null )
        {
            if (timeSinceLastDamage >= damageInterval)
            {
                damageOverTimeCoroutine = StartCoroutine(DamagePlayerOverTime(collision));
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerHealth>() != null)
        {
            if (damageOverTimeCoroutine != null)
                StopCoroutine(damageOverTimeCoroutine);
        }
    }
}
