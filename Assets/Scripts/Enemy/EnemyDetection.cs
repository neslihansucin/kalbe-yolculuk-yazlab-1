
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float viewAngle = 100f;
    [SerializeField] private float eyeHeight = 1f;
    [SerializeField] private LayerMask visionMask = ~0;

    public bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * eyeHeight;
        Vector3 direction = target - origin;

        if (direction.magnitude > detectionRange)
            return false;

        if (Vector3.Angle(transform.forward, direction) > viewAngle / 2f)
            return false;

        if (Physics.Raycast(
            origin,
            direction.normalized,
            out RaycastHit hit,
            direction.magnitude,
            visionMask,
            QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player ||
                   hit.transform.IsChildOf(player);
        }

        return false;
    }
}
