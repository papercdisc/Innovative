using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private PlayerHealth playerHealth;

    [Header("Audio Sources")]
    [SerializeField] [Tooltip("Reference 1st Audio Source (DO NOT CHANGE ORDER!!)")] private AudioSource heartbeatAudio;
    [SerializeField] [Tooltip("Reference 2nd Audio Source (DO NOT CHANGE ORDER!!)")] private AudioSource playerHitAudio;

    [Header("Heartbeat Audio Settings")]
    [SerializeField] private float lowHealthThreshold = 0.3f; 
    [SerializeField] private float highHealthThreshold = 0.95f;
    [SerializeField] private float pitchMultiplier = 3f; // How much the pitch increases as health gets lower/higher
    [SerializeField] private float pitchCap = 5f; // Cap the pitch increase to prevent it from getting too high

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealth = PlayerHealth.Instance;

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHeartbeatAudio);
            playerHealth.OnDamageTaken.AddListener(PlayPlayerHitAudio);
        }
    }

    public void PlayPlayerHitAudio(bool isHealAttack)
    {
        if (playerHitAudio != null)
        {
            playerHitAudio.pitch = Random.Range(0.8f, 1.2f);

            playerHitAudio.Play();
        }
    }

    private void UpdateHeartbeatAudio(float newHealth)
    {
        if (newHealth <= playerHealth.maxHealth * lowHealthThreshold || newHealth > playerHealth.maxHealth * highHealthThreshold)
        {
            if (!heartbeatAudio.isPlaying)
            {
                heartbeatAudio.Play();
            }

            // increase speed as health gets lower/higher
            float healthPercentage = newHealth / playerHealth.maxHealth;

            if (healthPercentage <= lowHealthThreshold)
            {
                heartbeatAudio.pitch = 1f + (lowHealthThreshold - healthPercentage) * pitchMultiplier; // Speed up as health decreases
            }
            else if (healthPercentage > highHealthThreshold)
            {
                heartbeatAudio.pitch = 1f + (healthPercentage - highHealthThreshold) * pitchMultiplier; // Speed up as health increases

                if(playerHealth.currentOverhealth > 0)
                {
                    heartbeatAudio.pitch += (playerHealth.currentOverhealth / playerHealth.maxHealth) * pitchMultiplier; // Speed up even more if we have overhealth
                    if (heartbeatAudio.pitch > pitchCap) // Cap the pitch increase to prevent it from getting too high
                    {
                        heartbeatAudio.pitch = pitchCap;
                    }
                }
            }
        }
        else
        {
            if (heartbeatAudio.isPlaying)
            {
                heartbeatAudio.Stop();
            }
        }
    }
}
