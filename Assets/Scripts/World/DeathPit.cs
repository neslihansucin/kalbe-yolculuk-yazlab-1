
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DeathPit : MonoBehaviour
{
    [Header("Oyuncu Respawn")]
    [SerializeField] private Transform respawnPoint;

    [Header("Kedi Respawn")]
    [SerializeField] private Transform cat;
    [SerializeField] private Transform catRespawnPoint;
    [SerializeField] private NavMeshAgent catAgent;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (respawnPoint == null)
        {
            Debug.LogWarning("RespawnPoint atanmamis!");
            return;
        }

        CharacterController controller =
            other.GetComponentInParent<CharacterController>();

        Transform player = controller != null
            ? controller.transform
            : other.transform;

        // Kadin karakteri baslangica tasi
        if (controller != null)
            controller.enabled = false;

        player.SetPositionAndRotation(
            respawnPoint.position,
            respawnPoint.rotation
        );

        if (controller != null)
            controller.enabled = true;

        Debug.Log("Kadin baslangica dondu.");

        // Kedi de ayni olayda baslangica donsun
        RespawnCat();
    }

    private void RespawnCat()
    {
        if (cat == null || catRespawnPoint == null)
        {
            Debug.LogError(
                "Cat veya CatRespawnPoint baglanmamis!"
            );
            return;
        }

        Vector3 oldPosition = cat.position;
        Vector3 destination = catRespawnPoint.position;

        NavMeshAgent agent = catAgent;

        if (agent == null)
            agent = cat.GetComponent<NavMeshAgent>();

        if (agent != null &&
            agent.isActiveAndEnabled &&
            agent.isOnNavMesh)
        {
            NavMeshHit hit;

            if (!NavMesh.SamplePosition(
                destination,
                out hit,
                2f,
                agent.areaMask))
            {
                Debug.LogWarning(
                    "Kedi icin uygun NavMesh noktasi bulunamadi."
                );
                return;
            }

            agent.ResetPath();

            if (!agent.Warp(hit.position))
            {
                Debug.LogWarning(
                    "Kedi NavMesh ile isinamadi."
                );
                return;
            }

            destination = hit.position;
            cat.rotation = catRespawnPoint.rotation;
        }
        else
        {
            CharacterController catController =
                cat.GetComponent<CharacterController>();

            bool controllerWasEnabled =
                catController != null &&
                catController.enabled;

            if (controllerWasEnabled)
                catController.enabled = false;

            Rigidbody catBody =
                cat.GetComponent<Rigidbody>();

            if (catBody != null)
            {
                catBody.position = destination;
                catBody.rotation =
                    catRespawnPoint.rotation;
            }

            cat.SetPositionAndRotation(
                destination,
                catRespawnPoint.rotation
            );

            if (controllerWasEnabled)
                catController.enabled = true;

            Physics.SyncTransforms();
        }

        Debug.Log(
            $"Kedi eski konum: {oldPosition}, " +
            $"yeni konum: {cat.position}"
        );

        StartCoroutine(CheckCatPosition(destination));
    }

    private IEnumerator CheckCatPosition(
        Vector3 expectedPosition)
    {
        // Diger hareket scriptlerinin calismasini bekle
        yield return null;

        if (cat == null)
            yield break;

        Debug.Log(
            $"Kedi sonraki kare konumu: {cat.position}"
        );

        if (Vector3.Distance(
            cat.position,
            expectedPosition) > 1f)
        {
            Debug.LogWarning(
                "Kedi isilandiktan sonra yeniden tasindi. " +
                "Kedinin hareket scriptini kontrol et."
            );
        }
    }
}
