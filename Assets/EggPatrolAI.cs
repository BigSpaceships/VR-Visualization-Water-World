using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//public enum AnimalState {
//    Idle,
//    Moving,
//}

//[RequireComponent(typeof(NavMeshAgent))]

public class EggPatrolAI : MonoBehaviour
{
    //Random Patrol
    GameObject player;

    NavMeshAgent agent;

    [SerializeField] LayerMask groundLayer, playerLayer;

    //patrol
    Vector3 destPoint;
    bool WalkpointSet;
    [SerializeField] float range;

    ////Random Animation

    //[Header("Idle1")]
    //[SerializeField] private float idle1Time = 53f;

    //[Header("Idle2")]
    //[SerializeField] private float idle2Time = 86f;

    //[Header("Jump")]
    //[SerializeField] private float jumpTime = 24f;

    //[Header("Run1")]
    //[SerializeField] private float runTime = 12f;

    //protected Animator animator;
    //protected AnimalState currentState = AnimalState.Idle;

    // Start is called before the first frame update
    void Start()
    {
        //InitialiseAnimal();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
    }

    //protected virtual void InitialiseAnimal() {
    //    animator = transform.GetChild(0).GetComponent<Animator>();  
    //}

    //protected void SetState(AnimalState newState) {

    //}

    //protected virtual void OnStateChange(AnimalState newState) {

    //}

    // Update is called once per frame
    void Update()
    {
        Patrol();
    }

    void Patrol() {
        if (!WalkpointSet) SearchForDest();
        if (WalkpointSet) agent.SetDestination(destPoint);
        if(Vector3.Distance(transform.position, destPoint) < 10) WalkpointSet = false;
    }

    void SearchForDest() {
        float z = Random.Range(-range, range);
        float x = Random.Range(-range, range);

        destPoint = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

        if (Physics.Raycast(destPoint, Vector3.down, groundLayer)) {
            WalkpointSet = true;
        }
    }

}
