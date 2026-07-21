using System.Collections.Generic;
using UnityEngine;

public class BombBehavior : MonoBehaviour
{
    // on spawn, shoot out and slowly decay speed
    // when speed is under a certain threshold, signal it will explode, then explode after a short delay
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D bombCollider;

    [Header("Movement Settings")]
    public Vector2 velocity;
    public float initialSpeed;
    private float currentSpeed;
    public float decayRate;

    [SerializeField] Transform stickTarget;

    [Header("Explosion Settings")]
    public float explodeThreshold;
    public float explodeDelay;

    float colEnableBuffer = 0.08f;

    [SerializeField] GameObject explosionChild;

    void Start()
    {
        bombCollider.enabled = false;

        currentSpeed = initialSpeed;
        rb.linearVelocity = velocity.normalized * currentSpeed;

        if (explosionChild != null)
        {
            explosionChild.SetActive(false);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 newVel = velocity.normalized;

        colEnableBuffer -= Time.fixedDeltaTime;
        if (colEnableBuffer <= 0 && !bombCollider.enabled)
        {
            bombCollider.enabled = true;
        }

        if (currentSpeed > 0)
        {
            currentSpeed -= decayRate * Time.fixedDeltaTime;
        }
        else
        {
            currentSpeed = 0;
        }

        rb.linearVelocity = newVel * currentSpeed;

        PullToTarget();

        if (rb.linearVelocity.magnitude <= explodeThreshold && explosionChild != null && !explosionChild.activeInHierarchy)
        {
            explosionChild.SetActive(true);
        }
    }

    private void PullToTarget()
    {
        // circle cast to find nearby enemies. If there are any, pull towards the closest one. If there aren't, do nothing.
        List<Transform> potentialTargets = new List<Transform>();

        RaycastHit2D[] hits = Physics2D.CircleCastAll(rb.position, 3.77f, Vector2.zero);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null && hits[i].collider.gameObject.GetComponent<EnemyBehavior>() != null)
            {
                potentialTargets.Add(hits[i].collider.transform);
                break;
            }
        }

        if (potentialTargets.Count > 0)
        {
            Transform closestTarget = potentialTargets[0];
            float closestDistance = Vector2.Distance(rb.position, closestTarget.position);
            for (int i = 1; i < potentialTargets.Count; i++)
            {
                float distance = Vector2.Distance(rb.position, potentialTargets[i].position);
                if (distance < closestDistance)
                {
                    closestTarget = potentialTargets[i];
                    closestDistance = distance;
                }
            }
            stickTarget = closestTarget;
        }

        // smoothly pull towards the target
        if (stickTarget != null)
        {
            Vector2 dir = (Vector2)stickTarget.position - rb.position;
            float distance = dir.magnitude;
            Vector2 newVel = dir.normalized * currentSpeed;
            if (distance < 0.2f)
            {
                rb.position = stickTarget.position;
                newVel = Vector2.zero;
            }
            rb.linearVelocity = newVel;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<EnemyAI>() != null)
        {
            currentSpeed *= 0.2f;
            stickTarget = collision.gameObject.transform;

            return;
        }

        if (collision.gameObject != null && collision.gameObject.GetComponent<EnemyAI>() == null)
        {
            velocity = Vector2.Reflect(velocity, collision.contacts[0].normal);
            currentSpeed += 1f;
        }
    }
}
