using UnityEngine;

public class EnemyAudioManager : MonoBehaviour
{
    [SerializeField] AudioSource enemyHitAudio;
    //[SerializeField] AudioSource enemyDeathAudio;

    [SerializeField] AudioClip deathClip;

    public void PlayEnemyHitAudio()
    {
        if (enemyHitAudio != null)
        {
            enemyHitAudio.pitch = Random.Range(0.8f, 1.2f);
    
            enemyHitAudio.Play();
        }
    }
    public void PlayEnemyDeathAudio()
    {
        Vector3 pos = transform.position;

        AudioSource.PlayClipAtPoint(deathClip, pos);
    }
}
