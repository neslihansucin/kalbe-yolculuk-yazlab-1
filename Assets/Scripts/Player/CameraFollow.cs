
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Transform catTarget;

    public Vector3 normalOffset = new Vector3(0f, 5f, -8f);
    public Vector3 crouchOffset = new Vector3(0f, 3.5f, -8f);
    public Vector3 catOffset = new Vector3(0f, 2.5f, -4f);

    public float smoothSpeed = 5f;

    [Header("Kamera Sinirlari")]
    public bool useCameraLimits = true;
    public Vector2 xLimits = new Vector2(-15f, 15f);
    public Vector2 zLimits = new Vector2(-15f, 15f);

    [Header("Kamera Engel Kontrolu")]
    [SerializeField] private LayerMask obstacleLayers = ~0;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private float orbitStep = 15f;

    private Transform playerTarget;
    private bool watchingCat = false;

    private readonly RaycastHit[] hits = new RaycastHit[64];
    private readonly Collider[] overlaps = new Collider[32];

    void Start()
    {
        playerTarget = target;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 offset;

        if (watchingCat)
        {
            offset = catOffset;
        }
        else
        {
            bool crouching =
                Keyboard.current != null &&
                Keyboard.current.leftCtrlKey.isPressed;

            offset = crouching ? crouchOffset : normalOffset;
        }

        Vector3 worldOffset = target.rotation * offset;

        Vector3 lookPosition = target.position +
            Vector3.up * (watchingCat ? 0.7f : 1.5f);

        // Sabit mesafede guvenli kamera konumu ara
        bool found;
        Vector3 safePosition = FindSafePosition(
            lookPosition,
            worldOffset,
            out found
        );

        float t = 1f - Mathf.Exp(
            -smoothSpeed * Time.deltaTime
        );

        if (found)
        {
            Vector3 currentOffset =
                transform.position - target.position;

            Vector3 desiredOffset =
                safePosition - target.position;

            if (currentOffset.sqrMagnitude < 0.001f)
                currentOffset = desiredOffset;

            // Kameranin uzakligini koruyarak
            // acisini yumusak degistir
            Vector3 smoothOffset = Vector3.Slerp(
                currentOffset.normalized,
                desiredOffset.normalized,
                t
            ) * desiredOffset.magnitude;

            Vector3 smoothPosition =
                target.position + smoothOffset;

            // Ara konum engelin icindeyse
            // dogrudan guvenli konuma gec
            transform.position =
                IsSafe(lookPosition, smoothPosition)
                ? smoothPosition
                : safePosition;
        }
        // Guvenli aci yoksa kamera mevcut
        // konumunda kalir, oyuncuya yaklasmaz

        Vector3 direction =
            lookPosition - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                t
            );
        }
    }

    private Vector3 FindSafePosition(
        Vector3 lookPosition,
        Vector3 worldOffset,
        out bool found)
    {
        Vector3 desired = target.position + worldOffset;

        if (IsSafe(lookPosition, desired))
        {
            found = true;
            return desired;
        }

        float step = Mathf.Max(5f, orbitStep);

        // Oyuncuya yaklasmadan saga veya sola don
        for (float angle = step; angle <= 180f; angle += step)
        {
            Vector3 rightOffset =
                Quaternion.AngleAxis(angle, Vector3.up)
                * worldOffset;

            Vector3 rightPosition =
                target.position + rightOffset;

            if (IsSafe(lookPosition, rightPosition))
            {
                found = true;
                return rightPosition;
            }

            Vector3 leftOffset =
                Quaternion.AngleAxis(-angle, Vector3.up)
                * worldOffset;

            Vector3 leftPosition =
                target.position + leftOffset;

            if (IsSafe(lookPosition, leftPosition))
            {
                found = true;
                return leftPosition;
            }
        }

        found = false;
        return transform.position;
    }

    private bool IsSafe(Vector3 origin, Vector3 position)
    {
        // Harita sinirlarini kontrol et
        if (useCameraLimits && !watchingCat)
        {
            if (position.x < xLimits.x ||
                position.x > xLimits.y ||
                position.z < zLimits.x ||
                position.z > zLimits.y)
            {
                return false;
            }
        }

        // Kamera konumu bir nesnenin icinde mi?
        int overlapCount = Physics.OverlapSphereNonAlloc(
            position,
            collisionRadius,
            overlaps,
            obstacleLayers,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < overlapCount; i++)
        {
            if (!IsOwnCollider(overlaps[i]))
                return false;
        }

        Vector3 direction = position - origin;
        float distance = direction.magnitude;

        if (distance < 0.001f)
            return true;

        // Kamera ile hedef arasinda engel var mi?
        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            collisionRadius,
            direction.normalized,
            hits,
            distance,
            obstacleLayers,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hitCount; i++)
        {
            if (!IsOwnCollider(hits[i].collider))
                return false;
        }

        return true;
    }

    private bool IsOwnCollider(Collider col)
    {
        if (col == null)
            return true;

        if (playerTarget != null &&
            col.transform.IsChildOf(playerTarget))
            return true;

        if (catTarget != null &&
            col.transform.IsChildOf(catTarget))
            return true;

        return false;
    }

    public void WatchCat()
    {
        if (catTarget == null)
            return;

        watchingCat = true;
        target = catTarget;
    }

    public void WatchPlayer()
    {
        watchingCat = false;
        target = playerTarget;
    }
}
