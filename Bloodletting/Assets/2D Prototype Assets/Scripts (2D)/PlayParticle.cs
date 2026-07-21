using UnityEngine;

public class PlayParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystemToPlay;
    [SerializeField] bool isHealParticle;


    public void Play()
    {
        if (particleSystemToPlay != null)
        {
            particleSystemToPlay.Play();
        }
    }

    public void PlayBool(bool recievedBool)
    {
        if (particleSystemToPlay != null && recievedBool == isHealParticle)
        {
            particleSystemToPlay.Play();
        }
    }
}
