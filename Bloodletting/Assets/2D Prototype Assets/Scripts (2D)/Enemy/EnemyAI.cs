using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    private EnemyType enemyType;

    #region Steering Parameters
    [Header("Steering")]
    [SerializeField] private List<Detector> detectors;
    [SerializeField] private List<SteeringBehavior> steeringBehaviors;
    [SerializeField] private AIData aiData;
    [SerializeField] private ContextSolver moveDirSolver;
    #endregion

    [Header("Behavior Settings")]
    [SerializeField]
    private float detectionDelay = 0.05f, aiUpdateDelay = 0.06f, attackDelay = 1f;

    [SerializeField] private float attackDist = 0.5f;

    // health thresholds
    [SerializeField] private float playerHealthThreshold = 0.7f; // for saboteur behavior, runs away if player health is above this percentage
    [SerializeField] private float healthThreshold = 0.3f; // for coward behavior, runs away if enemy health is below this percentage

    // state variables
    [SerializeField] EnemyState currentEnemyState;
    [SerializeField] bool shouldAvoidPlayer = false;

    // movement vector
    [SerializeField] private Vector2 movementInput;

    [Header("Events")]
    public UnityEvent<Transform, bool> onAttack;
    public UnityEvent<Vector2, bool> onMove;

    void Start()
    {
        InvokeRepeating("PerformDetection", 0f, detectionDelay); // more optimized for performance apparently, as it doesn't run this every frame
    }

    private void PerformDetection()
    {
        //bool playerIsDanger = false; // just keeping it false for now. this will change with the implementation of more specific behaviors.
        foreach (Detector detector in detectors)
        {
            detector.Detect(aiData, shouldAvoidPlayer);
        }

        float[] danger = new float[8];
        float[] interest = new float[8];

        foreach (SteeringBehavior steeringBehavior in steeringBehaviors)
        {
            (danger, interest) = steeringBehavior.GetSteering(danger, interest, aiData);
        }
    }

    private void Update()
    {
        switch (enemyType)
        {
            case EnemyType.Chaser:
                UpdateChaser();
                break;
            case EnemyType.Coward:
                UpdateCoward();
                break;
            case EnemyType.Saboteur:
                UpdateSaboteur();
                break;
            case EnemyType.Healer:
                UpdateHealer();
                break;
        }
    }

    // Enemy type-specific update functions
    private void UpdateChaser() // most basic enemy. just chases player in range.
    {
        bool shouldStop = false;
        if (aiData.currentTarget != null) // if current target is in range, chase
        {
            if (currentEnemyState == EnemyState.Idle)
            {
                currentEnemyState = EnemyState.Aggro;
                StartCoroutine(ChaseAndAttack(false));
            }
        }
        else if(aiData.GetTargetsCount() > 0) // if there are valid targets in the list, find another
        {
            // choose closest target in list as current target
            aiData.currentTarget = aiData.targets[0];
        }
        else
        {
            shouldStop = true;
        }

        onMove?.Invoke(movementInput, shouldStop);
    }
    private void UpdateCoward() // runs away from player if low health, otherwise chases player in range
    {
    }
    private void UpdateSaboteur() // chases player, but runs away if player has high health/overhealth
    {
        bool shouldStop = false;
        if (PlayerHealth.Instance == null) return;

        if (PlayerHealth.Instance.currentHealth / PlayerHealth.Instance.maxHealth > playerHealthThreshold)
        {
            shouldAvoidPlayer = true;
            if (currentEnemyState != EnemyState.Flee)
            {
                currentEnemyState = EnemyState.Flee;
                StartCoroutine(Flee());
            }
        }
        else
        {
            shouldAvoidPlayer = false;

            if (aiData.currentTarget != null) // if current target is in range, chase
            {
                if (currentEnemyState == EnemyState.Idle)
                {
                    currentEnemyState = EnemyState.Aggro;
                    StartCoroutine(ChaseAndAttack(false));
                }
            }
            else if (aiData.GetTargetsCount() > 0) // if there are valid targets in the list, find another
            {
                // choose closest target in list as current target
                aiData.currentTarget = aiData.targets[0];
            }
            else
            {
                shouldStop = true;
            }
        }

        onMove?.Invoke(movementInput, shouldStop);
    }
    private void UpdateHealer() // same behavior as chaser, but heals player instead
    {
        bool shouldStop = false;
        if (aiData.currentTarget != null) // if current target is in range, chase
        {
            if (currentEnemyState == EnemyState.Idle)
            {
                currentEnemyState = EnemyState.Aggro;
                StartCoroutine(ChaseAndAttack(true));
            }
        }
        else if (aiData.GetTargetsCount() > 0) // if there are valid targets in the list, find another
        {
            // choose closest target in list as current target
            aiData.currentTarget = aiData.targets[0];
        }
        else
        {
            shouldStop = true;
        }

        onMove?.Invoke(movementInput, shouldStop);
    }

    private IEnumerator ChaseAndAttack(bool isHealAttack)
    {
        if(aiData.currentTarget == null)
        {
            // stopping logic, return to idle
            movementInput = Vector2.zero;
            currentEnemyState = EnemyState.Idle;
            yield break;
        }
        else
        {
            float dist = Vector2.Distance(aiData.currentTarget.position, transform.position);

            if (dist < attackDist) // attack if in range
            {
                // attack logic
                movementInput = Vector2.zero;
                onAttack?.Invoke(aiData.currentTarget, isHealAttack);
                yield return new WaitForSeconds(attackDelay); // wait to attack again
                StartCoroutine(ChaseAndAttack(isHealAttack));
            }
            else
            {
                // chase logic
                movementInput = moveDirSolver.GetDirectionToMove(steeringBehaviors, aiData);
                yield return new WaitForSeconds(aiUpdateDelay);
                StartCoroutine(ChaseAndAttack(isHealAttack));
            }
        }
    }

    private IEnumerator Flee()
    {
        if (!shouldAvoidPlayer)
        {
            if (aiData.currentTarget == null)
            {
                 // stopping logic, return to idle
                movementInput = Vector2.zero;
                currentEnemyState = EnemyState.Idle;
            }
            else
            {
                // if a target is still in range, start chasing again
                movementInput = moveDirSolver.GetDirectionToMove(steeringBehaviors, aiData);
                yield return new WaitForSeconds(aiUpdateDelay);
                StartCoroutine(ChaseAndAttack(false));
            }

            yield break;
        }

        // flee logic
        movementInput = moveDirSolver.GetDirectionToMove(steeringBehaviors, aiData);
        onMove?.Invoke(movementInput, false);
        yield return new WaitForSeconds(aiUpdateDelay);
        StartCoroutine(Flee());
    }

    public void OnDeath()
    {
        CancelInvoke("PerformDetection");
        StopAllCoroutines();
    }
}
