using UnityEngine;

/// <summary>
/// This class is a sister to TargetDetector. It finds obstacles near the enemy and stores their colliders in the AIDataClass.
/// </summary>
public class ObstacleDetector : Detector
{
    [SerializeField]
    private float detectionRadius = 2f;

    [SerializeField]
    private LayerMask obstaclesLayerMask, playerObstacleLayerMask;

    [SerializeField]
    private bool showGizmos = true;

    Collider2D[] colliders;

    public override void Detect(AIData aiData, bool playerIsDanger = false)
    {
        if (!playerIsDanger)
        {
            colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, obstaclesLayerMask);
        }
        else
        {
            colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerObstacleLayerMask);
        } 
        aiData.obstacles = colliders;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        if ( Application.isPlaying && colliders != null)
        {
            Gizmos.color = Color.red;

            foreach (Collider2D obsCol in colliders)
            {
                Gizmos.DrawSphere(obsCol.transform.position, 0.2f);
            }
        }
    }
}
