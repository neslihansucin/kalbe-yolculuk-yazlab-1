
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Transform catTarget;

    public Vector3 normalOffset = new Vector3(0f, 5f, -8f);
    public Vector3 crouchOffset = new Vector3(0f, 3.5f, -8f);

    // Kediye daha yakýn kamera
    public Vector3 catOffset = new Vector3(0f, 2.5f, -4f);

    public float smoothSpeed = 5f;
    [Header("Kamera Sinirlari")]
    public bool useCameraLimits = true;

    public Vector2 xLimits = new Vector2(-15f, 15f);
    public Vector2 zLimits = new Vector2(-15f, 15f);

    private Transform playerTarget;
    private bool watchingCat = false;

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
            bool crouching = Keyboard.current != null &&
                             Keyboard.current.leftCtrlKey.isPressed;

            offset = crouching ? crouchOffset : normalOffset;
        }

        Vector3 desiredPosition =
            target.position + target.rotation * offset;
        if (useCameraLimits && !watchingCat)
        {
            desiredPosition.x = Mathf.Clamp(
                desiredPosition.x,
                xLimits.x,
                xLimits.y
            );

            desiredPosition.z = Mathf.Clamp(
                desiredPosition.z,
                zLimits.x,
                zLimits.y
            );
        }

        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            t
        );

        Vector3 lookPosition = target.position +
            Vector3.up * (watchingCat ? 0.7f : 1.5f);

        Vector3 direction = lookPosition - transform.position;

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
