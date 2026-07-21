using UnityEngine;

public abstract class SteeringBehavior : MonoBehaviour
{
    public abstract (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData data);
}
