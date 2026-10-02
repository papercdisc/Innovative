using UnityEngine;

public class KnifeTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        gameObject.GetComponentInParent<KnifeProjectile>().PickupTrigger(other);
    }
}
