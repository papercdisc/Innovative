using UnityEngine;
using UnityEngine.Events;

public class CamTrigger : MonoBehaviour
{
    public UnityEvent<Transform, float> onTriggerEnter;
    [SerializeField] Collider2D triggerCollider;

    [SerializeField] private Transform roomTarget;
    [SerializeField] private float zoom = 5.77f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (onTriggerEnter != null)
        {
            if(collision.GetComponent<PlayerHealth>() != null)
            {
                onTriggerEnter?.Invoke(roomTarget, zoom);
            }
        }
    }
}
