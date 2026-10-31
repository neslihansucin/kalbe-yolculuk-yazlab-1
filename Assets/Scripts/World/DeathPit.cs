
using UnityEngine;

public class DeathPit : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (respawnPoint == null)
        {
            Debug.LogWarning("RespawnPoint atanmamýþ!");
            return;
        }

        CharacterController controller =
            other.GetComponentInParent<CharacterController>();

        Transform player = controller != null
            ? controller.transform
            : other.transform;

        if (controller != null)
            controller.enabled = false;

        player.position = respawnPoint.position;
        player.rotation = respawnPoint.rotation;

        if (controller != null)
            controller.enabled = true;
    }
}
