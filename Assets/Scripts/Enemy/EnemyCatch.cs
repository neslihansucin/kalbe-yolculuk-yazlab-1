
using UnityEngine;
using UnityEngine.AI;

public class EnemyCatch : MonoBehaviour
{
    [Header("Baþlangýç Noktalarý")]
    [SerializeField] private Transform playerStartPoint;
    [SerializeField] private Transform cat;
    [SerializeField] private Transform catStartPoint;

    [Header("Düþman Ayarlarý")]
    [SerializeField] private float catchDistance = 1.2f;

    private Transform player;
    private CharacterController playerController;
    private PlayerInventory playerInventory;
    private EnemyChase enemyChase;
    private NavMeshAgent catAgent;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("Player bulunamadý!");
            return;
        }

        player = playerObject.transform;
        playerController =
            playerObject.GetComponent<CharacterController>();
        playerInventory =
            playerObject.GetComponent<PlayerInventory>();

        enemyChase = GetComponent<EnemyChase>();

        if (cat != null)
            catAgent = cat.GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (player == null || playerStartPoint == null)
            return;

        // Küre varsa oyuncu korunur
        if (playerInventory != null && playerInventory.HasSphere)
            return;

        Vector3 enemyPosition = transform.position;
        Vector3 playerPosition = player.position;

        enemyPosition.y = 0;
        playerPosition.y = 0;

        if (Vector3.Distance(enemyPosition, playerPosition)
            <= catchDistance)
        {
            ReturnPlayerToStart();
        }
    }

    private void ReturnPlayerToStart()
    {
        // Oyuncuyu baþlangýca gönder
        if (playerController != null)
            playerController.enabled = false;

        player.position = playerStartPoint.position;
        player.rotation = playerStartPoint.rotation;

        if (playerController != null)
            playerController.enabled = true;

        // Kediyi de baþlangýca gönder
        ReturnCatToStart();

        // Düþman devriyeye döner
        if (enemyChase != null)
            enemyChase.StopChasing();

        Debug.Log("Oyuncu ve kedi baþlangýca döndü.");
    }

    private void ReturnCatToStart()
    {
        if (cat == null || catStartPoint == null)
        {
            Debug.LogWarning(
                "Kedi veya kedi baþlangýç noktasý atanmadý!");
            return;
        }

        if (catAgent != null && catAgent.isActiveAndEnabled)
        {
            // NavMeshAgent ile güvenli ýþýnlama
            if (!catAgent.Warp(catStartPoint.position))
            {
                Debug.LogWarning(
                    "Kedi NavMesh üzerinde baþlangýca ýþýnlanamadý!");
                return;
            }

            // Eski yolu temizle
            if (catAgent.isOnNavMesh)
                catAgent.ResetPath();
        }
        else
        {
            cat.position = catStartPoint.position;
        }

        cat.rotation = catStartPoint.rotation;

        Debug.Log("Kedi baþlangýca döndü.");
    }
}
