using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 150f;

    public float normalHeight = 2f;
    public float crouchHeight = 1f;

    private CharacterController controller;
    private Vector2 moveInput;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float mouseX = Mouse.current.delta.x.ReadValue();

        transform.Rotate(0f, mouseX * rotationSpeed * Time.deltaTime, 0f);

        Vector3 move =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        controller.Move(move * speed * Time.deltaTime);

        if (Keyboard.current.leftCtrlKey.isPressed)
        {
            controller.height = crouchHeight;
        }
        else
        {
            controller.height = normalHeight;
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}