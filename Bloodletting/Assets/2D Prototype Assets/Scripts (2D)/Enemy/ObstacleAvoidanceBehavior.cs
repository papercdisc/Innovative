using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The meat and potatoes. 
/// </summary>
public class ObstacleAvoidanceBehavior : SteeringBehavior
{
    [SerializeField] private float radius = 2f, agentColliderRadius = 0.5f;

    [SerializeField] private bool showGizmos = true;

    // gizmo parameters
    float[] dangersResultTemp = null;

    // returns weighted danger and interest values for each direction
    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData data) 
    {
        foreach (Collider2D obsstacleCollider in data.obstacles)
        {
            if (obsstacleCollider == null) break;

            Vector2 dirToObs = obsstacleCollider.ClosestPoint(transform.position) - (Vector2)transform.position;
            float distToObs = dirToObs.magnitude;

            // calculate weight based on distance
            float weight 
                = distToObs <= agentColliderRadius ? 1f // if we're colliding, max weight
                : (radius - distToObs) / radius; // otherwise, weight decreases linearly with distance

            if (obsstacleCollider.gameObject.layer == LayerMask.NameToLayer("Bomb"))
            {
                weight *= 0.5f;
            }

            Vector2 dirToObsNormalized = dirToObs.normalized;

            // add obstacle parameters to danger array
            for (int i = 0; i < Directions.eightDirections.Count; i++)
            {
                float result = Vector2.Dot(dirToObsNormalized, Directions.eightDirections[i]);

                float valueToAdd = weight * result;

                // override value if its higher than currently stored value
                if (valueToAdd > danger[i])
                {
                    danger[i] = valueToAdd;
                }
            }
        }
        dangersResultTemp = danger; // for gizmos

        return (danger, interest);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (Application.isPlaying && dangersResultTemp != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < dangersResultTemp.Length; i++)
            {
                Gizmos.DrawRay(
                    transform.position, 
                    Directions.eightDirections[i] * dangersResultTemp[i] // visualize the weight applied to danger in each direction
                    );
            }
        }
    }
}

public static class Directions
{
    public static List<Vector2> eightDirections = new List<Vector2>
    {
        new Vector2(0, 1).normalized, // up
        new Vector2(1, 1).normalized, // up-right
        new Vector2(1, 0).normalized, // right
        new Vector2(1, -1).normalized, // down-right
        new Vector2(0, -1).normalized, // down
        new Vector2(-1, -1).normalized, // down-left
        new Vector2(-1, 0).normalized, // left
        new Vector2(-1, 1).normalized // up-left
    };
}
