using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] BombBehavior bombBehavior;
    [SerializeField] float explosionDamage = 20;
    float explosionTimer = 0.6f;
    bool hasExploded = false;
    bool destroyTimerStarted = false;

    [SerializeField] private float explosionRadius = 3.77f;
    [SerializeField] private GameObject indicator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        indicator.transform.localScale = Vector3.zero;
        bombBehavior = this.GetComponentInParent<BombBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasExploded)
        {
            explosionTimer -= Time.deltaTime;

            // Smoothly scale the indicator up to the explosion radius over the explosion timer duration
            indicator.transform.localScale = Vector3.Lerp(indicator.transform.localScale, Vector3.one * explosionRadius, Time.deltaTime / explosionTimer);

            if (explosionTimer <= 0)
            {
                indicator.transform.localScale = Vector3.one * explosionRadius;
                Explode();
            }
        }
        if (hasExploded && !destroyTimerStarted)
        {
            destroyTimerStarted = true;

            // Destroy the explosion object after a short delay to allow for any explosion effects to play out
            Destroy(bombBehavior.gameObject, 0.5f);
        }
    }

    void Explode()
    {
        hasExploded = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded)
        {
            if (collision.gameObject.GetComponent<PlayerHealth>() != null)
            {
                //Debug.Log("OnTriggerEnter: Player hit by explosion!");
                collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(explosionDamage);
            }
            if (collision.gameObject.GetComponent<EnemyHealth>() != null)
            {
                collision.gameObject.GetComponent<EnemyHealth>().TakeDamage(explosionDamage);
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (hasExploded)
        {
            if (collision.gameObject.GetComponent<PlayerHealth>() != null)
            {
                //Debug.Log("OnTriggerStay: Player hit by explosion!");
                collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(explosionDamage);

                Destroy(bombBehavior.gameObject);
            }
            if (collision.gameObject.GetComponent<EnemyHealth>() != null)
            {
                collision.gameObject.GetComponent<EnemyHealth>().TakeDamage(explosionDamage);
                Destroy(bombBehavior.gameObject);
            }
        }
    }
}
