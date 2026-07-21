using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bars")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider overhealthBar;
    [SerializeField] private Slider overhealthTimer;

    [Header("Overlay Settings")]
    [SerializeField] private Image damageOverlay;
    [SerializeField] private float maxAlpha = 0.4f;
    [SerializeField] private float currentAlpha = 0f;

    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float highHealthThreshold = 0.9f;


    private PlayerHealth playerHealth;
    private void Start()
    {
        playerHealth = PlayerHealth.Instance;

        if (playerHealth != null)
        {
            // set max values
            healthBar.maxValue = playerHealth.maxHealth;
            overhealthBar.maxValue = playerHealth.maxHealth; // might change later, just for current UI configuration
            overhealthTimer.maxValue = playerHealth.overhealthSafeTime;

            // set initial values
            healthBar.value = playerHealth.currentHealth;
            overhealthBar.value = playerHealth.currentOverhealth;

            if (playerHealth.currentOverhealth > 0)
            {
                overhealthBar.gameObject.SetActive(true);
            }
            else
            {
                overhealthBar.gameObject.SetActive(false);
            }

            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
            playerHealth.OnOverhealthChanged.AddListener(UpdateOverhealthBar);
        }
    }

    private void Update()
    {
        UpdateOverhealthTimer();
    }

    public void UpdateHealthBar(float newValue)
    {
        UpdateDamageOverlay(playerHealth.currentHealth);
        healthBar.value = newValue;
    }
    public void UpdateOverhealthBar(float newValue)
    {
        UpdateDamageOverlay(playerHealth.currentHealth);

        if (newValue > 0)
        {

            overhealthBar.value = newValue;
            overhealthBar.gameObject.SetActive(true);
        }
        else
        {
            overhealthBar.value = 0;
            overhealthBar.gameObject.SetActive(false);
        }
    }
    public void UpdateOverhealthTimer()
    {
        if (playerHealth.currentOverhealth > 0)
        {
            overhealthTimer.gameObject.SetActive(true);
            overhealthTimer.value = playerHealth.activeOverhealthSafeTime;
        }
        else
        {
            overhealthTimer.gameObject.SetActive(false);
        }
    }

    public void UpdateDamageOverlay(float healthValue)
    {
        float healthPercentage = healthValue / playerHealth.maxHealth;

        if (healthPercentage <= lowHealthThreshold)
        {
            // find proper alpha based on how low the health is
            currentAlpha = Mathf.Lerp(maxAlpha, 0f, healthPercentage / lowHealthThreshold);
        }
        else if (healthPercentage >= highHealthThreshold)
        {
            float alphaMulti = 0.3f; // highest percentage of max alpha to reach before overhealth;

            // find proper alpha based on how high the health is
            if(playerHealth.currentOverhealth > 0)
            {
                currentAlpha = Mathf.Lerp(maxAlpha * alphaMulti, 0f, (playerHealth.currentOverhealth / (playerHealth.maxHealth * 1.5f)) * 2f); // if we have overhealth, make the overlay more intense (up to double the max alpha)
            }
            else
            {
                currentAlpha = Mathf.Lerp(0f, maxAlpha * alphaMulti, (healthPercentage - highHealthThreshold) / (1f - highHealthThreshold));
            }
        }
        else
        {
            currentAlpha = 0f;
        }

        damageOverlay.color = new Color(damageOverlay.color.r, damageOverlay.color.g, damageOverlay.color.b, currentAlpha);
    }
}
