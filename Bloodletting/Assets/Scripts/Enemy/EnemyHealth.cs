using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : Health
{
    public UnityEvent OnTakeDamage;
    public UnityEvent OnDeath;
    public float destoryDelay = 3f;
    public bool isDead;

    [Header("Health Stats")]
    [field: SerializeField] public float maxHealth { get; private set; } = 100f;
    [field: SerializeField] public float currentHealth { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public override void TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        OnTakeDamage?.Invoke();

        if (currentHealth <= 0) {
            Die();
        }
    }
    public override void Die()
    {
        if (isDead) return;

        isDead = true;
        OnDeath?.Invoke();
        //Destroy(gameObject);
        Destroy(gameObject, destoryDelay);
    }
}
