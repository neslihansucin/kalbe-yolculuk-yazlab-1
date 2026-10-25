
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 0.15f;

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

        if (Mouse.current != null)
        {
            float mouseX = Mouse.current.delta.x.ReadValue();

            transform.Rotate(
                0f,
                mouseX * rotationSpeed,
                0f
            );
        }

        // A ve D ile oyuncuyu döndür
        float turnInput = moveInput.x;

        transform.Rotate(
            0f,
            turnInput * 120f * Time.deltaTime,
            0f
        );

        // W ve S ile ileri-geri hareket et
        Vector3 move = transform.forward * moveInput.y;

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
