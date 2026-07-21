using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is a sister to ObstacleDetector. It finds targets (in this case, the player) and stores their locations in the AIData class.
/// </summary>
public class TargetDetector : Detector
{
    [SerializeField]
    private float targetDetectionRange = 5f;

    [SerializeField]
    private LayerMask obstaclesLayerMask, playerLayerMask;

    [SerializeField]
    private bool showGizmos = true;

    private List<Transform> colliders;

    public override void Detect(AIData aiData, bool playerIsDanger = false)
    {
        if (playerIsDanger)
        {
            aiData.targets = null;
            return;
        }

        // Implement target detection logic here
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, targetDetectionRange, playerLayerMask);

        if (playerCollider != null)
        {
            // check if player is visible
            Vector2 dir = (playerCollider.transform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, targetDetectionRange, obstaclesLayerMask);

            // make sure that the collider we see is on the "Player" layer
            if (hit.collider != null && (playerLayerMask & (1 << hit.collider.gameObject.layer)) != 0)
            {
                Debug.DrawRay(transform.position, dir * targetDetectionRange, Color.magenta);
                colliders = new List<Transform> { playerCollider.transform };
            }
            else
            {
                colliders = null;
            }
        }
        else
        {
            // player is not visible
            colliders = null;
        }
        aiData.targets = colliders;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.DrawWireSphere(transform.position, targetDetectionRange);

        if (colliders ==null) return;

        Gizmos.color = Color.magenta;
        foreach(var item in colliders)
        {
            Gizmos.DrawSphere(item.position, 0.2f);
        }
    }
}
