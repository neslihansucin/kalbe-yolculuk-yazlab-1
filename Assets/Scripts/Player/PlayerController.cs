
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using Unity.AI.Navigation;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 0.15f;

    public float normalHeight = 2f;
    public float crouchHeight = 1f;

    [Header("Karakter Animasyonu")]
    [SerializeField] private Animator playerAnimator;

    [Header("NavMesh Ayarlari")]
    [SerializeField] private NavMeshSurface playerSurface;
    [SerializeField] private float navMeshSearchDistance = 1.5f;
    [SerializeField] private float maxHorizontalOffset = 0.2f;

    private CharacterController controller;
    private Vector2 moveInput;
    private NavMeshQueryFilter navFilter;
    private bool navMeshReady;
    [SerializeField] private float gravity = -9.81f;
    private float verticalVelocity = 0f;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int CrouchHash =
        Animator.StringToHash("IsCrouching");

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerSurface == null)
        {
            Debug.LogError(
                "Player Surface bos! HeartFloor nesnesini bagla."
            );
            return;
        }

        navFilter = new NavMeshQueryFilter
        {
            agentTypeID = playerSurface.agentTypeID,
            areaMask = NavMesh.AllAreas
        };

        navMeshReady = true;

        Debug.Log("Player NavMesh kontrolu hazir.");
    }

    void Update()
    {
        // Mouse ile donme
        if (Mouse.current != null)
        {
            float mouseX = Mouse.current.delta.x.ReadValue();

            transform.Rotate(
                0f,
                mouseX * rotationSpeed,
                0f
            );
        }

        // A ve D ile donme
        float turnInput = moveInput.x;

        transform.Rotate(
            0f,
            turnInput * 120f * Time.deltaTime,
            0f
        );

        // W ve S ile hareket
        Vector3 move = transform.forward * moveInput.y;

        Vector3 movement =
            move * speed * Time.deltaTime;

        Vector3 previousPosition = transform.position;

        if (navMeshReady &&
            movement.sqrMagnitude > 0.000001f)
        {
            MoveOnNavMesh(movement);
        }
        // Player yere bassýn, havada asýlý kalmasýn
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move(
            Vector3.up * verticalVelocity * Time.deltaTime
        );
        // Egilme kontrolu
        bool isCrouching =
            Keyboard.current != null &&
            Keyboard.current.leftCtrlKey.isPressed;

        controller.height =
            isCrouching ? crouchHeight : normalHeight;

        // Animasyon kontrolu
        if (playerAnimator != null)
        {
            Vector3 actualMovement =
                transform.position - previousPosition;

            actualMovement.y = 0f;

            float movementSpeed =
                Mathf.Clamp01(
                    actualMovement.magnitude /
                    Mathf.Max(speed * Time.deltaTime, 0.0001f)
                );

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

    private void MoveOnNavMesh(Vector3 movement)
    {
        // CharacterController'in ayak seviyesini kullan
        Vector3 start = new Vector3(
            transform.position.x,
            controller.bounds.min.y + 0.1f,
            transform.position.z
        );

        NavMeshHit startHit;

        if (!NavMesh.SamplePosition(
            start,
            out startHit,
            navMeshSearchDistance,
            navFilter))
        {
            return;
        }

        // Uzak bir NavMesh adasina tutunmayi engelle
        Vector3 startOffset = startHit.position - start;
        startOffset.y = 0f;

        if (startOffset.magnitude > maxHorizontalOffset)
            return;

        Vector3 target = startHit.position + movement;

        NavMeshHit edgeHit;

        bool blocked = NavMesh.Raycast(
            startHit.position,
            target,
            out edgeHit,
            navFilter
        );

        if (!blocked)
        {
            TryMove(movement, startHit.position);
            return;
        }

        // Sinira carparsa sinir boyunca kaymayi dene
        Vector3 normal = edgeHit.normal;
        normal.y = 0f;

        if (normal.sqrMagnitude < 0.0001f)
            return;

        normal.Normalize();

        Vector3 slideMovement =
            Vector3.ProjectOnPlane(movement, normal);

        slideMovement.y = 0f;

        if (slideMovement.sqrMagnitude > 0.000001f)
        {
            TryMove(slideMovement, startHit.position);
        }
    }

    private void TryMove(Vector3 movement, Vector3 navStart)
    {
        Vector3 target = navStart + movement;

        NavMeshHit targetHit;

        if (!NavMesh.SamplePosition(
            target,
            out targetHit,
            navMeshSearchDistance,
            navFilter))
        {
            return;
        }

        // Hedef NavMesh'in yakininda olmali
        Vector3 offset = targetHit.position - target;
        offset.y = 0f;

        if (offset.magnitude > maxHorizontalOffset)
            return;

        // Arada NavMesh siniri var mi?
        NavMeshHit pathHit;

        if (NavMesh.Raycast(
            navStart,
            targetHit.position,
            out pathHit,
            navFilter))
        {
            return;
        }

        // Fiziksel collider carpisma kontrolu korunur
        controller.Move(movement);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
