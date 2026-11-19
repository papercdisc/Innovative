using UnityEngine;
using UnityEngine.Events;

public class PlayerHUDAnim2D : MonoBehaviour
{
    public PlayerKnifeCombatManager cm;
    Animator animator;
    [SerializeField] float slowAmount = 0.75f;
    bool slowActive;

    public UnityEvent<bool> OnFinisherAnimActive;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        if(PlayerCombatFP.Instance != null) // legacy script
        {        
            PlayerCombatFP.Instance.OnADS.AddListener(ADSStatus);
            PlayerCombatFP.Instance.OnKnifeThrown.AddListener(() => animator.SetTrigger("OnThrow"));
            PlayerCombatFP.Instance.OnFinisher.AddListener(() => animator.SetTrigger("OnFinisher"));
        }
        else if(cm != null)
        {        
            cm.OnADS.AddListener(ADSStatus);
            cm.AttackHandler.OnKnifeThrown.AddListener(() => animator.SetTrigger("OnThrow"));
            cm.RetrieveHandler.OnFinisher.AddListener(() => animator.SetTrigger("OnFinisher"));
        }
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
        else if(cm != null && cm.RetrieveHandler != null)
        {
            cm.RetrieveHandler.ResolveFinisher();
        }
    }
    private void OnDisable() => EndFinisher();

    void ADSStatus(bool isAiming)
    {
        //animator.SetTrigger(isAiming ? "OnAim" : "ReturnToIdle");

        animator.SetBool("ADS", isAiming);
    }
}
