using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AnimalState {
    Idle,
    Moving,
}

[RequireComponent(typeof(NavMeshAgent))]

public class Animal : MonoBehaviour
{
    [Header("Wander")]
    public float wanderDistance = 50f;
    public float walkSpeed = 5f;
    public float maxWalkTime = 6f;

    [Header("Idle")]
    public float idleTime = 5f;

    protected NavMeshAgent navMeshAgent;
    protected Animator animator;
    protected AnimalState currentState = AnimalState.Idle;

    private void Start() {
        InitialiseAnimal();
    }

    protected virtual void InitialiseAnimal() {
        animator = transform.GetChild(0).GetChild(0).GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = walkSpeed;

        currentState = AnimalState.Idle;
        UpdateState();
    }

    protected virtual void UpdateState() {
        switch (currentState) {
            case AnimalState.Idle:
                HandleIdleState();
                break;
            case AnimalState.Moving:
                HandleMovingState();
                break;
        }
    }

    protected Vector3 GetRandomNavMeshPosition(Vector3 origin, float distance) {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;
        NavMeshHit navMeshHit;

        if (NavMesh.SamplePosition(randomDirection,out navMeshHit, distance, NavMesh.AllAreas)) {
            return navMeshHit.position;
        } else {
            return GetRandomNavMeshPosition(origin, distance);
        }
    }

    protected virtual void HandleIdleState() {
        StartCoroutine(WaitToMove());
    }

    private IEnumerator WaitToMove() {
        float waitTime = Random.Range(idleTime / 2, idleTime * 2);
        yield return new WaitForSeconds(waitTime);

        Vector3 randomDestination = GetRandomNavMeshPosition(transform.position, wanderDistance);

        navMeshAgent.SetDestination(randomDestination);
        SetState(AnimalState.Moving);
    }

    protected virtual void HandleMovingState() {
        StartCoroutine(WaitToReachDestination().ToString());
    }

    private IEnumerable WaitToReachDestination() {
        float startTime = Time.time;

        while (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance) {
            if (Time.time - startTime >= maxWalkTime) {
                navMeshAgent.ResetPath();
                SetState(AnimalState.Idle);
                yield break;
            }

            yield return null;
        }

        SetState(AnimalState.Idle);

    }

    protected void SetState(AnimalState newState) {
        if (currentState == newState)
            return;

        currentState = newState;
        OnStateChanged(newState);
    }

    protected virtual void OnStateChanged(AnimalState newState) {
        animator?.CrossFadeInFixedTime(newState.ToString(), 0.5f);


        UpdateState();
    }

}
