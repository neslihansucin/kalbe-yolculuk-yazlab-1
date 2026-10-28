
using UnityEngine;

public class DialogueMovementLock : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private PlayerController playerMovement;
    [SerializeField] private MonoBehaviour cameraLook;

    private bool isLocked = false;
    private bool wasMovementEnabled;
    private bool wasCameraEnabled;

    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    void Update()
    {
        if (dialoguePanel == null)
            return;

        bool dialogueOpen = dialoguePanel.activeInHierarchy;

        if (dialogueOpen && !isLocked)
        {
            LockMovement();
        }
        else if (!dialogueOpen && isLocked)
        {
            UnlockMovement();
        }
    }

    void LockMovement()
    {
        isLocked = true;

        if (playerMovement != null)
        {
            wasMovementEnabled = playerMovement.enabled;
            playerMovement.enabled = false;
        }

        if (cameraLook != null)
        {
            wasCameraEnabled = cameraLook.enabled;
            cameraLook.enabled = false;
        }

        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UnlockMovement()
    {
        if (playerMovement != null)
            playerMovement.enabled = wasMovementEnabled;

        if (cameraLook != null)
            cameraLook.enabled = wasCameraEnabled;

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;

        isLocked = false;
    }

    void OnDisable()
    {
        if (isLocked)
            UnlockMovement();
    }
}
