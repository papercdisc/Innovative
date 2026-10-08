using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Enemy3D : MonoBehaviour
{
    NavMeshAgent agent;
    EnemyHealth enemyHealth;

    public List<Vector3> pathPoints = new List<Vector3>();
    int currentPathIndex = 0;
    public float turnSpeed = 165f; // degrees per second
    public float angleToMoveThreshold = 15f; // degrees

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if(pathPoints.Count > 0)
            agent.SetDestination(pathPoints[currentPathIndex]);

        agent.updateRotation = false;
        enemyHealth = GetComponent<EnemyHealth>();
        enemyHealth.OnDeath.AddListener(() => agent.isStopped = true); // stop the agent when the enemy dies
    }

    // Update is called once per frame
    void Update()
    {
        if(pathPoints == null || pathPoints.Count == 0)
            return;

        if(agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            currentPathIndex = (currentPathIndex + 1) % pathPoints.Count; // this (%) operator ensures that the index wraps around to 0 when it reaches the end of the list
            agent.SetDestination(pathPoints[currentPathIndex]);

            return;
        }

        Vector3 dir = agent.steeringTarget - transform.position;
        dir.y = 0; // keep the direction strictly horizontal

        if (dir.sqrMagnitude > 0.001f) // avoid zero-length direction
        {
            // constantly rotate towards the target direction
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            // if the difference between current and target rotation is greater than the threshold, stop moving and wait until facing the target direction
            float angle = Quaternion.Angle(transform.rotation, targetRotation);
            bool shouldWait = angle > angleToMoveThreshold;

            agent.isStopped = shouldWait;
            if (shouldWait)
            {
                agent.velocity = Vector3.zero; // stop instantly
            }
        }
    }

    public void SetTarget(Vector3 target)
    {
        agent.SetDestination(target);
    }
}
