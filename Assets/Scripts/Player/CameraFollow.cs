using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 normalOffset = new Vector3(0f, 5f, -8f);
    public Vector3 crouchOffset = new Vector3(0f, 3.5f, -8f);

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 offset = Keyboard.current.leftCtrlKey.isPressed
            ? crouchOffset
            : normalOffset;

        transform.position = target.position + target.rotation * offset;

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}