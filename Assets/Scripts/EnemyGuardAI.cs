using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyGuardAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Patrol / Guard")]
    public List<Transform> patrolPoints = new List<Transform>();
    public float patrolSpeed = 2f;
    public float guardWaitTime = 2f;
    public float stoppingDistance = 0.5f;

    [Header("Detection")]
    public float spotDistance = 12f;
    public float shootingDistance = 8f;
    public float fieldOfView = 90f;
    public LayerMask wallLayer;

    [Header("Combat")]
    [SerializeField] private WeaponController weaponController;

    private NavMeshAgent agent;
    private int patrolIndex = 0;
    private bool isWaiting = false;
    private float stunEndTime;

    public bool IsStunned => Time.time < stunEndTime;

    public void Stun(float duration)
    {
        if (duration <= 0f) return;
        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);
    }

    enum EnemyState
    {
        Patrol,
        Standby,
        Chase,
        Shoot
    }

    private EnemyState currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (weaponController == null)
        {
            weaponController = GetComponent<WeaponController>();
        }

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }

        agent.speed = patrolSpeed;
        currentState = EnemyState.Patrol;

        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused)
        {
            if (agent.hasPath) agent.isStopped = true;
            return;
        }

        if (player == null) return;

        if (IsStunned)
        {
            if (agent != null && agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            return;
        }

        bool canSeePlayer = CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (canSeePlayer && distanceToPlayer <= shootingDistance)
        {
            currentState = EnemyState.Shoot;
        }
        else if (canSeePlayer && distanceToPlayer <= spotDistance)
        {
            currentState = EnemyState.Chase;
        }
        else if (!isWaiting)
        {
            currentState = EnemyState.Patrol;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolUpdate();
                break;

            case EnemyState.Standby:
                break;

            case EnemyState.Chase:
                ChasePlayer();
                break;

            case EnemyState.Shoot:
                ShootMode();
                break;
        }
    }

    void PatrolUpdate()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed;

        if (patrolPoints.Count == 0)
        {
            Debug.LogWarning(gameObject.name + " has no patrol points.");
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= stoppingDistance)
        {
            StartCoroutine(StandbyThenMove());
        }
    }

    IEnumerator StandbyThenMove()
    {
        isWaiting = true;
        currentState = EnemyState.Standby;

        agent.isStopped = true;

        yield return new WaitForSeconds(guardWaitTime);

        patrolIndex++;

        if (patrolIndex >= patrolPoints.Count)
        {
            patrolIndex = 0;
        }

        GoToNextPatrolPoint();

        isWaiting = false;
        currentState = EnemyState.Patrol;
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Count == 0) return;

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed + 1.5f;

        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
        agent.SetDestination(targetPosition);

        LookAtPlayer();
    }

    void ShootMode()
    {
        agent.isStopped = true;

        LookAtPlayer();

        if (weaponController == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        weaponController.TryFire(direction);
    }

    bool CanSeePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > spotDistance)
        {
            return false;
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > fieldOfView / 2f)
        {
            return false;
        }

        Vector3 rayStart = transform.position + Vector3.up * 1.5f;
        Vector3 rayDirection = (player.position + Vector3.up - rayStart).normalized;

        if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, spotDistance))
        {
            if (hit.transform == player)
            {
                return true;
            }

            if (((1 << hit.transform.gameObject.layer) & wallLayer) != 0)
            {
                return false;
            }
        }

        return false;
    }

    void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}