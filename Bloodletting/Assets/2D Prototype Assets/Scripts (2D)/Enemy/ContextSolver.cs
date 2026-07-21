using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this and related scripts reference this article: https://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter18_Context_Steering_Behavior-Driven_Steering_at_the_Macro_Scale.pdf
// as well as Sunny Valley Studio's Context Steering tutorial series on youtube

public class ContextSolver : MonoBehaviour
{
    [SerializeField]
    private bool showGizmo = true;

    // gizmo parameters
    float[] interestGizmo = new float[0];
    Vector2 resultDir = Vector2.zero;
    private float rayLength = 1;

    void Start()
    {
        interestGizmo = new float[8];   
    }

    public Vector2 GetDirectionToMove(List<SteeringBehavior> behaviors, AIData data)
    {
        float[] danger = new float[8];
        float[] interest = new float[8];

        // loop through each behavior
        foreach (SteeringBehavior behavior in behaviors) // get the danger and interest values for each direction to store in temp arrays
        {
            (danger, interest) = behavior.GetSteering(danger, interest, data);
        }

        bool hasTarget = data.GetTargetsCount() > 0;
        float highestDanger = 0;

        if (hasTarget)
        {
            // subtract danger values from interest array
            // (this is so that, if there is an obstacle in the same direction as the interest, the avoidance behavior wins out)
            for (int i = 0; i < 8; i++)
            {
                interest[i] = Mathf.Clamp01(interest[i] - danger[i]);
            }
        }
        else
        {
            // if we're close to an obstacle, avoid it, otherwise, stop
            for (int i = 0; i < 8; i++)
            {
                interest[i] = Mathf.Clamp01(1 - danger[i]);

                if (danger[i] > highestDanger)
                {
                    highestDanger = danger[i];
                }
            }
        }
        interestGizmo = interest;

        // get average direction (so movement isn't choppy)
        Vector2 outputDir = Vector2.zero;
        for (int i = 0; i < 8; i++) // loop through eight directions
        {
            outputDir += Directions.eightDirections[i] * interest[i]; // multiply each direction by interest, and add said direction to output
        }
        outputDir.Normalize(); // normalize to isolate direction

        //Debug.Log("has target?: " + hasTarget);
        //Debug.Log("highest danger: " + highestDanger);
        if (highestDanger < 0.3f && !hasTarget) // if we're not close to an obstacle and we don't have a target, stop
        {
            outputDir = Vector2.zero;
        }

        resultDir = outputDir;

        // return movement direction
        return resultDir;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && showGizmo)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, resultDir * rayLength);
        }
    }
}
