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
    private float minDistance = 6f; // the minimum distance before the player is seen
    private float maxDistance = 12f; // the maximum distance before the player has escaped
    private float attackDistance = 1.8f;
    private float searchWaitTimer;
    private float patrolWaitTime;
    private float patrolWaitTimer;
    private float retreatCoolDownTime = 8f; // time until enemy can start looking for player after losing them in a chase
    private float retreatCoolDownTimer;
    private bool isPatroling;
    private bool patrolTimerStarted; // used to timer is only started once
    private State currentState;
    private State lastState;
    private Vector3 playerLastPos; // used when searching for player
    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController playerScript;


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
        playerDistance = Vector3.Distance(player.transform.position, gameObject.transform.position);
        StateCheck();
    }

    private void StateCheck()
    {
        switch (currentState)
        {
            case State.idle:
                if (isPatroling) currentState = State.patrolling;
                modelColor = Color.grey;
                CheckChase();
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
                CheckChase();
                if (!agent.hasPath && !agent.pathPending) // enemy has finished current path
                {
                    lastState = State.patrolling;
                    currentState = State.idle;
                }
                break;

            case State.chasing:
                modelColor = Color.red;
                agent.SetDestination(player.transform.position);
                if(playerDistance <= attackDistance) // enemy attacks player
                {
                    agent.isStopped = true;
                    currentState = State.attacking;
                }
                else agent.isStopped = false;
                if(playerDistance > maxDistance) // enemy lost player
                {
                    playerLastPos = player.transform.position;
                    agent.SetDestination(playerLastPos);
                    currentState = State.searching;
                }
                break;

            case State.searching:
                modelColor = Color.blue;
                CheckChase();
                if (!agent.hasPath && !agent.pathPending) // enemy has finished current path
                {
                    searchWaitTimer += Time.deltaTime;
                    if (searchWaitTimer > 3f)
                    {
                        retreatCoolDownTimer = 0f;
                        currentState = State.retreating;
                    }
                }
                break;

            case State.attacking:
                agent.isStopped = true;
                modelColor = Color.yellow;
                playerScript.TakeDamage(Random.Range(10f, 15f));
                currentState = State.chasing;
                break;

            case State.retreating:
                modelColor = Color.green;
                Patrol();
                retreatCoolDownTimer += Time.deltaTime;
                if(retreatCoolDownTimer >= retreatCoolDownTime)
                {
                    Debug.Log("Retreat Cooldown Over");
                    CheckChase();
                    currentState = State.patrolling;
                }
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

    private void CheckChase()
    {
        if (currentState == State.chasing || currentState == State.attacking) return;
        if(playerDistance < minDistance)
        {
            currentState = State.chasing;
        }
    }
}
