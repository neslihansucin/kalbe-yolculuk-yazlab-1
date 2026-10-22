
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 150f;

    public float normalHeight = 2f;
    public float crouchHeight = 1f;

    [Header("Karakter Animasyonu")]
    [SerializeField] private Animator playerAnimator;

    private CharacterController controller;
    private Vector2 moveInput;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int CrouchHash =
        Animator.StringToHash("IsCrouching");

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Mouse ile dönme
        float mouseX = Mouse.current.delta.x.ReadValue();

        transform.Rotate(
            0f,
            mouseX * rotationSpeed * Time.deltaTime,
            0f
        );

        // Oyuncu hareketi
        Vector3 move =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        move = Vector3.ClampMagnitude(move, 1f);

        controller.Move(move * speed * Time.deltaTime);

        // Eðilme kontrolü
        bool isCrouching =
            Keyboard.current.leftCtrlKey.isPressed;

        if (isCrouching)
        {
            controller.height = crouchHeight;
        }
        else
        {
            controller.height = normalHeight;
        }

        // Animasyon kontrolü
        if (playerAnimator != null)
        {
            float movementSpeed = move.magnitude;

            playerAnimator.SetFloat(
                SpeedHash,
                movementSpeed
            );

            playerAnimator.SetBool(
                CrouchHash,
                isCrouching
            );
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
