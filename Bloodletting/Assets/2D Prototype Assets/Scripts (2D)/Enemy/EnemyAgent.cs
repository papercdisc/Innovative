using UnityEngine;

public class EnemyAgent : MonoBehaviour
{
    // References
    EnemyHealth enemyHealth;
    Rigidbody2D rb;

    [SerializeField] float moveSpeed;
    [SerializeField] float stopSpeed = 1.2f;
    Vector2 moveDir;
    bool stopped = false;

    [SerializeField] float damageAmount = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (moveDir != Vector2.zero)
        {
            rb.linearVelocity = moveDir * moveSpeed;
        }
        else if( moveDir == Vector2.zero && stopped) 
        {
            // lerp to stop
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, stopSpeed * Time.fixedDeltaTime);
        }
    }

    public void SetMoveDir(Vector2 dir, bool isStopped)
    {
        moveDir = dir;
        moveDir.Normalize();
        stopped = isStopped;
    }

    public void AttackTarget(Transform target, bool isHealAttack)
    {
        //Debug.Log("I AM ACCESSINGG THIS ATTACK FUNCTION!");

        if (target.gameObject.GetComponent<Health>() == null) { Debug.Log("IDK where the health script is!"); return;}

        if (target.gameObject.GetComponent<PlayerHealth>() != null)
        {
            float damageToDeal = isHealAttack ? -damageAmount : damageAmount; // deal negative damage if it's a heal attack

            //Debug.Log("I should be doing damage but im not cause im a fucking idiot");
            target.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageToDeal);
        }
    }
}
