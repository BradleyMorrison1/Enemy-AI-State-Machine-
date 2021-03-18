using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public GameObject player;
    public GameObject model;
    public GameObject[] patrolPoints;
    public NavMeshAgent agent;
    public Material modelMaterial;
    public Color modelColor;
    private int currentPatrolPoint; // used to cycle through patrol points
    private int patrolPointIncrement = 1; // added to the current patrol point to cycle
    private float playerDistance;
    private float patrolWaitTime;
    private float patrolWaitTimer;
    private bool isPatroling;
    private bool patrolTimerStarted; // used to timer is only started once
    private State currentState;
    private State lastState;

    enum State
    {
        idle,
        patrolling,
        chasing,
        searching,
        attacking,
        retreating
    }

    void Start()
    {
        currentPatrolPoint = 0;
        currentState = State.patrolling;
        
        ColorUtility.TryParseHtmlString("FF5A5A", out modelColor);
    }

    void Update()
    {
        modelMaterial.color = modelColor;
        StateCheck();
    }

    private void StateCheck()
    {
        switch (currentState)
        {
            case State.idle:
                if (isPatroling) currentState = State.patrolling;
                modelColor = Color.grey;
                if (lastState == State.patrolling)
                {
                    patrolWaitTime = Random.Range(4f, 6f);
                    StartPatrolTimer();
                    patrolTimerStarted = true;
                    if(patrolWaitTimer >= patrolWaitTime)
                    {
                        patrolTimerStarted = false;
                        ChangePatrol();
                        currentState = State.patrolling;
                    }
                }
                break;

            case State.patrolling:
                modelColor = Color.white;
                Patrol();
                if(!agent.hasPath && !agent.pathPending)
                {
                    lastState = State.patrolling;
                    currentState = State.idle;
                }
                break;

            case State.chasing:

                break;

            case State.searching:

                break;

            case State.attacking:

                break;

            case State.retreating:

                break;
        }
    }

    private void Patrol()
    {
        if (patrolPoints == null) return; // prevents script from running if there are no patrol points
        
        agent.SetDestination(patrolPoints[currentPatrolPoint].transform.position);
    }

    private void ChangePatrol()
    {
        currentPatrolPoint += patrolPointIncrement;

        if (currentPatrolPoint >= patrolPoints.Length - 1) patrolPointIncrement *= (-1);
        if (currentPatrolPoint < 0)
        {
            currentPatrolPoint = 0;
            patrolPointIncrement *= (-1);
        }
        
        Debug.Log(currentPatrolPoint);
    }

    private void StartPatrolTimer()
    {
        if (!patrolTimerStarted) patrolWaitTimer = 0f;
        patrolWaitTimer += Time.deltaTime;
    }
}
