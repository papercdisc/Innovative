using UnityEngine;
using UnityEngine.Events;

public class PlayerHUDAnim2D : MonoBehaviour
{
    Animator animator;
    [SerializeField] float slowAmount = 0.75f;
    bool slowActive;

    public UnityEvent<bool> OnFinisherAnimActive;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        PlayerCombatFP.Instance.OnADS.AddListener(ADSStatus);
        PlayerCombatFP.Instance.OnKnifeThrown.AddListener(() => animator.SetTrigger("OnThrow"));
        PlayerCombatFP.Instance.OnFinisher.AddListener(() => animator.SetTrigger("OnFinisher"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FinisherSlow(int num)
    {
        if (num == 1) {
            OnFinisherAnimActive?.Invoke(true);

            slowActive = true;         
            Time.timeScale = slowAmount;
        }
        else if (num == 0) {
            EndFinisher();
        }
    }
    public void EndFinisher()
    {
        if (slowActive)
        {
            slowActive = false;
            Time.timeScale = 1;
        }

        OnFinisherAnimActive?.Invoke(false);
        if(PlayerCombatFP.Instance != null) PlayerCombatFP.Instance.ResolveFinisher();
    }
    private void OnDisable() => EndFinisher();

    void ADSStatus(bool isAiming)
    {
        //animator.SetTrigger(isAiming ? "OnAim" : "ReturnToIdle");

        animator.SetBool("ADS", isAiming);
    }
}
