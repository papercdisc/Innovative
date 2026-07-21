using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] Image healthFillImage;

    [SerializeField] private EnemyHealth enemyHealth;

    [SerializeField] private Color healthMaxColor;
    [SerializeField] private Color healthMinColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar.maxValue = enemyHealth.maxHealth;
        healthBar.value = enemyHealth.currentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealthBar(enemyHealth.currentHealth);
    }

    public void UpdateHealthBar(float newValue)
    {
        healthBar.value = newValue;
        healthFillImage.color = Color.Lerp(healthMinColor, healthMaxColor, newValue / enemyHealth.maxHealth);
    }
}
