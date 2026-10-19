
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EnemyDetectionZone : MonoBehaviour
{
    [SerializeField] private Transform player;

    private BoxCollider zoneCollider;

    public bool IsPlayerInside { get; private set; }

    private void Awake()
    {
        zoneCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (player == null)
        {
            IsPlayerInside = false;
            return;
        }

        Vector3 localPosition =
            transform.InverseTransformPoint(player.position);

        Vector3 center = zoneCollider.center;
        Vector3 halfSize = zoneCollider.size * 0.5f;

        IsPlayerInside =
            Mathf.Abs(localPosition.x - center.x) <= halfSize.x &&
            Mathf.Abs(localPosition.y - center.y) <= halfSize.y &&
            Mathf.Abs(localPosition.z - center.z) <= halfSize.z;
    }
}
