using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : Health
{
    public UnityEvent OnTakeDamage;
    public UnityEvent OnDeath;

    [Header("Health Stats")]
    [field: SerializeField] public float maxHealth { get; private set; } = 100f;
    [field: SerializeField] public float currentHealth { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        OnTakeDamage?.Invoke();

        if (currentHealth < 0) {
            Die();
        }
    }
    public override void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
