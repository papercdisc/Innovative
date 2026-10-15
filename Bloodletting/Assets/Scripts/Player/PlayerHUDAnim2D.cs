using UnityEngine;

public class PlayerHUDAnim2D : MonoBehaviour
{
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        PlayerCombatFP.Instance.OnADS.AddListener(ADSStatus);
        PlayerCombatFP.Instance.OnKnifeThrown.AddListener(() => animator.SetTrigger("OnThrow"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ADSStatus(bool isAiming)
    {
        //animator.SetTrigger(isAiming ? "OnAim" : "ReturnToIdle");

        animator.SetBool("ADS", isAiming);
    }
}
