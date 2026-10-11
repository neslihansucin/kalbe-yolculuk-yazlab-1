
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChase : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private EnemyDetectionZone detectionZone;
    [SerializeField] private EnemyDetection enemyDetection;
    [SerializeField] private EnemyPatrol enemyPatrol;
    [SerializeField] private float chaseSpeed = 7f;
    [SerializeField] private float patrolSpeed = 2f;

    private NavMeshAgent agent;
    private bool isChasing = false;
    private bool sphereCollected = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!agent.isOnNavMesh ||
            player == null ||
            detectionZone == null ||
            enemyDetection == null ||
            enemyPatrol == null)
            return;

        if (sphereCollected)
            return;

        if (Time.frameCount % 120 == 0)
        {
            Debug.Log("Oyuncu bölgede mi? " +
                      detectionZone.IsPlayerInside);

            Debug.Log("Düþman oyuncuyu görüyor mu? " +
                      enemyDetection.CanSeePlayer());

            Debug.Log("Kovalama aktif mi? " + isChasing);
        }


        // Oyuncu ilk kez görüldüðünde kovalamaya baþla
        if (!isChasing &&
            detectionZone.IsPlayerInside &&
            enemyDetection.CanSeePlayer())
        {
            StartChasing();
        }

        // Bir kez gördükten sonra kovalamayý býrakma
        if (isChasing)
        {
            agent.SetDestination(player.position);
        }
    }

    private void StartChasing()
    {
        isChasing = true;
        enemyPatrol.enabled = false;
        agent.speed = chaseSpeed;
    }

    public void StopChasing()
    {
        isChasing = false;
        agent.speed = patrolSpeed;
        agent.ResetPath();
        enemyPatrol.enabled = true;
    }

    public void OnSphereCollected()
    {
        sphereCollected = true;
        StopChasing();
    }
}
