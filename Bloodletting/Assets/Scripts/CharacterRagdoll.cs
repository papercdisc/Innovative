using UnityEngine;

public class CharacterRagdoll : MonoBehaviour
{
    private Rigidbody[] boneRBs;

    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boneRBs = GetComponentsInChildren<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        SetRagdollActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateRagdoll()
    {
        SetRagdollActive(true);
    }
    private void SetRagdollActive(bool isActive)
    {
        if(animator != null)
        {
            animator.enabled = !isActive; // turn off animator when ragdolling
        }
        foreach(Rigidbody rb in boneRBs)
        {
            rb.isKinematic = !isActive; // turn on physics for ragdoll bones
        }
    }
}
