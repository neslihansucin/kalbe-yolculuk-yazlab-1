
using UnityEngine;

public class EnemyCatch : MonoBehaviour
{
    [SerializeField] private Transform playerStartPoint;
    [SerializeField] private float catchDistance = 1.2f;

    private Transform player;
    private CharacterController playerController;
    private PlayerInventory playerInventory;
    private EnemyChase enemyChase;

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
        playerController = playerObject.GetComponent<CharacterController>();
        playerInventory = playerObject.GetComponent<PlayerInventory>();
        enemyChase = GetComponent<EnemyChase>();
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
        if (playerController != null)
            playerController.enabled = false;

        player.position = playerStartPoint.position;
        player.rotation = playerStartPoint.rotation;

        if (playerController != null)
            playerController.enabled = true;

        // Yakalandýktan sonra düþman devriyeye döner
        if (enemyChase != null)
            enemyChase.StopChasing();

        Debug.Log("Oyuncu yakalandý! Baþlangýca döndü.");
    }
}
