using System.Linq;
using UnityEngine;

public class SeekBehavior : SteeringBehavior
{
    [SerializeField] 
    private float targetReachThreshold = 0.5f;

    [SerializeField] 
    private bool showGizmo = true;

    bool reachedLastTarget = true; // for selecting next follow target

    // gizmo parameters
    private Vector2 targetPosCached;
    private float[] interestsTemp;

    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData data)
    {
        if (reachedLastTarget)
        {
            if(data.targets == null || data.targets.Count <= 0)
            {
                data.currentTarget = null;
                return (danger, interest);
            }
            else
            {
                reachedLastTarget = false;
                data.currentTarget = data.targets.OrderBy
                    (target => Vector2.Distance(target.position, transform.position)).FirstOrDefault(); // select closest target
            }
        }

        // cache last position if target is seen (so if target goes out of LOS, enemy can navigate to last seen position)
        if (data.currentTarget != null && data.targets != null && data.targets.Contains(data.currentTarget))
        {
            targetPosCached = data.currentTarget.position;
        }

        // check if target has been reached
        if (Vector2.Distance(transform.position, targetPosCached) < targetReachThreshold)
        {
            reachedLastTarget = true;
            data.currentTarget = null;
            return (danger, interest);
        }

        // if we havent reached the target, find interest directions
        Vector2 dirToTarget = (targetPosCached - (Vector2)transform.position).normalized;
        for (int i = 0; i < interest.Length; i++)
        {
            float result = Vector2.Dot(dirToTarget, Directions.eightDirections[i]);

            // only accept directions at less than 90 degrees to the target direction
            if (result > 0) // if going in this direction result in moving closer to target
            {
                float valueToPutIn = result;
                if(valueToPutIn > interest[i])
                {
                    interest[i] = valueToPutIn;
                }
            }
        }
        interestsTemp = interest; // cache for gizmo
        return(danger, interest);
    }

    private void OnDrawGizmos()
    {
        if(!showGizmo) return;
        Gizmos.DrawSphere(targetPosCached, 0.2f);

        if (Application.isPlaying && interestsTemp != null)
        {
            Gizmos.color = Color.green;
            for(int i = 0; i < interestsTemp.Length; i++)
            {
                Gizmos.DrawRay(transform.position, Directions.eightDirections[i] * interestsTemp[i]);
            }
            if(reachedLastTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(targetPosCached, 0.1f);
            }
        }
    }
}
