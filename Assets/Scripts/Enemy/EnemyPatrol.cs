
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTime = 5f;

    private NavMeshAgent agent;
    private int currentPointIndex;
    private float waitTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("Devriye noktalarý atanmadý!");
            enabled = false;
            return;
        }

        GoToCurrentPoint();
    }

    private void OnEnable()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        waitTimer = 0f;

        if (patrolPoints != null && patrolPoints.Length > 0)
            GoToCurrentPoint();
    }


    private void Update()
    {
        if (!agent.isOnNavMesh || agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                currentPointIndex =
                    (currentPointIndex + 1) % patrolPoints.Length;

                waitTimer = 0f;
                GoToCurrentPoint();
            }
        }
        else
        {
            waitTimer = 0f;
        }
    }

    private void GoToCurrentPoint()
    {
        if (!agent.isOnNavMesh)
            return;

        if (patrolPoints[currentPointIndex] == null)
        {
            Debug.LogWarning("Devriye noktasý boþ!");
            return;
        }

        Vector3 target = patrolPoints[currentPointIndex].position;

        NavMeshPath path = new NavMeshPath();

        if (agent.CalculatePath(target, path) &&
            path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(target);
        }
        else
        {
            Debug.LogWarning("Düþman devriye noktasýna ulaþamýyor!");
        }
    }
}
