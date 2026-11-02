
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueMovementLock : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private PlayerController playerMovement;
    [SerializeField] private MonoBehaviour cameraLook;

    private bool isLocked = false;
    private bool movementReleased = false;

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

        // F6: Konusma ekrani acikken
        // oyuncunun hareket etmesine izin ver.
        if (dialogueOpen &&
            isLocked &&
            !movementReleased &&
            Keyboard.current != null &&
            Keyboard.current.f6Key.wasPressedThisFrame)
        {
            ReleaseForWalking();
        }
    }

    void LockMovement()
    {
        isLocked = true;
        movementReleased = false;

        if (playerMovement != null)
        {
            wasMovementEnabled = playerMovement.enabled;

            // Scripti kapatma!
            // Sadece hareketi durdur.
            playerMovement.SetMovementEnabled(false);
        }

        if (cameraLook != null)
        {
            wasCameraEnabled = cameraLook.enabled;

            CameraFollow follow =
                cameraLook as CameraFollow;

            if (follow != null)
            {
                // Kamera scripti acik kalir.
                // Yalnizca takip durdurulur.
                follow.SetDialogueActive(true);
            }
            else
            {
                // Diger kamera kontrol scriptleri.
                cameraLook.enabled = false;
            }
        }

        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // F6 ile hareketi geri ac.
    public void ReleaseForWalking()
    {
        if (!isLocked || movementReleased)
            return;

        movementReleased = true;

        if (playerMovement != null &&
            wasMovementEnabled)
        {
            playerMovement.enabled = true;
            playerMovement.SetMovementEnabled(true);
        }

        if (cameraLook != null)
        {
            cameraLook.enabled = wasCameraEnabled;

            CameraFollow follow =
                cameraLook as CameraFollow;

            if (follow != null && wasCameraEnabled)
            {
                follow.SetDialogueActive(false);
            }
        }

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;

        Debug.Log(
            "F6: Karakter ve kamera hareketi serbest."
        );
    }

    void UnlockMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = wasMovementEnabled;

            if (wasMovementEnabled)
            {
                playerMovement.SetMovementEnabled(true);
            }
        }

        if (cameraLook != null)
        {
            cameraLook.enabled = wasCameraEnabled;

            CameraFollow follow =
                cameraLook as CameraFollow;

            if (follow != null && wasCameraEnabled)
            {
                follow.SetDialogueActive(false);
            }
        }

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;

        isLocked = false;
        movementReleased = false;
    }

    void OnDisable()
    {
        if (isLocked)
        {
            UnlockMovement();
        }
    }
}
