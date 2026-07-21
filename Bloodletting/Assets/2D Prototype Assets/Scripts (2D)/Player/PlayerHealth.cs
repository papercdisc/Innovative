using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// We're going to opperate under the assumption that this game is in real time, so NOT turn based. 
/// Just for adaptability, real time health functions will have the prefix "RT", and turn based health functions will have the prefix "TB".
/// </summary>
public class PlayerHealth : Health
{
    public static PlayerHealth Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    [HideInInspector] public UnityEvent<float> OnHealthChanged;
    [HideInInspector] public UnityEvent<float> OnOverhealthChanged;
    
    public UnityEvent<bool> OnDamageTaken;
    public UnityEvent OnDeath;

    [Header("Health Stats")]
    [field: SerializeField] public float maxHealth { get; private set; } = 100f;
    [field: SerializeField] public float currentHealth { get; private set; }

    [Header("Overhealth")]
    [field: SerializeField] public float currentOverhealth { get; private set; } // health above max health
    [field: SerializeField] public float overhealthSafeTime { get; private set; } = 15; // amount of time it is safe to have overhealth before exploding (DO NOT CHANGE DURING RUNTIME)
    [field: SerializeField] public float activeOverhealthSafeTime { get; private set; }

    [SerializeField] private float overhealthTimerBuffer = 0.5f;
    [SerializeField] private float activeOverhealthTimerBuffer;

    [Header("Health Regeneration")]
    [SerializeField] private float regenRate = 5f; // Health points regenerated per second (DO NOT CHANGE DURING RUNTIME)
    [SerializeField] private float regenBufferTime = 3f;
    [SerializeField] private float activeRegenBuffer;

    [Header("Regen Modes")]
    public bool isConstant;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth / 2;
        activeOverhealthSafeTime = overhealthSafeTime;
    }

    // Update is called once per frame
    void Update()
    {
        RTHealthRegen();
        HandleOverhealth();
    }

    public void RTHealthRegen() // constant regen after not taking damage
    {
        if (activeRegenBuffer > 0)
        {
            activeRegenBuffer -= Time.deltaTime;
            return;
        }

        if (isConstant)
        {
            if (currentHealth < maxHealth)
            {
                currentHealth += regenRate * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
                OnHealthChanged?.Invoke(currentHealth);
            }
            else
            {
                currentHealth = maxHealth; // Ensure health does not exceed max health
                currentOverhealth += regenRate * Time.deltaTime; // Regenerate overhealth
                OnOverhealthChanged.Invoke(currentOverhealth);
            }
        }
        else
        {
            if (currentHealth < maxHealth)
            {
                currentHealth += regenRate;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
                OnHealthChanged?.Invoke(currentHealth);
            }
            else
            {
                currentHealth = maxHealth;
                currentOverhealth += regenRate; // Regenerate overhealth
                OnOverhealthChanged.Invoke(currentOverhealth);
            }


            if (activeRegenBuffer != regenBufferTime)
            {
                activeRegenBuffer = 3f; // Reset the buffer time after regeneration
            }
        }
    }

    public void HandleOverhealth()
    {
        if (currentOverhealth > 0)
        {
            if (activeOverhealthTimerBuffer > 0) // buffer time (set on hit)
            {
                activeOverhealthTimerBuffer -= Time.deltaTime;
                return;
            }

            activeOverhealthSafeTime -= Time.deltaTime;
            
            if (activeOverhealthSafeTime <= 0)
            {
                Die(); // Player dies if overhealth is not reduced in time
            }
        }
        else activeOverhealthSafeTime = overhealthSafeTime; // Reset the safe time when overhealth is reduced to 0
    }

    public override void TakeDamage(float dmg)
    {
        if (currentOverhealth > 0) 
        {
            currentOverhealth -= dmg;
            OnOverhealthChanged.Invoke(currentOverhealth);

            activeOverhealthTimerBuffer = overhealthTimerBuffer; // Reset the overhealth timer buffer after taking damage to overhealth

            if (currentOverhealth < 0)
            {
                currentHealth += currentOverhealth; // Apply remaining damage to health
                currentOverhealth = 0; // Reset overhealth to 0
                OnHealthChanged?.Invoke(currentHealth);
            }
        }
        else
        {
            if((currentHealth - dmg) > maxHealth) // If the damage would put us above max health, add to overhealth instead
            {
                float excessHealth = (currentHealth - dmg) - maxHealth;
                if (excessHealth > 0) { 
                    currentHealth = maxHealth; 
                    currentOverhealth += excessHealth;
                }
                else
                {
                    currentHealth -= dmg;
                }

                OnHealthChanged?.Invoke(currentHealth);
                OnOverhealthChanged?.Invoke(currentOverhealth);
            }
            else 
            { 
                currentHealth -= dmg;
                OnHealthChanged?.Invoke(currentHealth);

                if (currentHealth <= 0)
                {
                    currentHealth = 0; // Ensure health does not go below 0
                    Die();
                }
            }
        }

        bool isHealAttack = dmg < 0;

        OnDamageTaken?.Invoke(isHealAttack);
        activeRegenBuffer = regenBufferTime; // Reset the regeneration buffer time after taking damage
    }
    public override void Die()
    {
        // Implement death logic here
        //Debug.Log("Player has died.");
        OnDeath?.Invoke();
        Time.timeScale = 0f;
    }


    #region Debug Methods
    [ContextMenu("Take 15 Damage")]
    private void Take15Damage()
    {
        TakeDamage(15f);
    }
    #endregion
}
