
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueUI : MonoBehaviour
{
    [Header("Diyalog Arayuzu")]
    [SerializeField] private TMP_InputField playerInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_Text guardResponseText;
    [SerializeField] private GuardController guardController;
    [SerializeField] private GameObject dialoguePanel;

    [Header("Oyuncu ve Kamera")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Muhafizdan Uzaklasma")]
    [SerializeField] private Transform guardTransform;
    [SerializeField] private float exitDistance = 4f;

    private bool isWaitingForResponse = false;
    private bool isDialogueOpen = false;
    private bool movementReleased = false;
    private bool isClosing = false;

    private int dialogueVersion = 0;

    private void OnEnable()
    {
        if (cameraFollow != null)
        {
            cameraFollow.OnReturnToMovementRequested +=
                OnTabMovementRequested;
        }
    }

    private void OnDisable()
    {
        if (cameraFollow != null)
        {
            cameraFollow.OnReturnToMovementRequested -=
                OnTabMovementRequested;
        }
    }

    private void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClicked);
        playerInput.onSubmit.AddListener(OnInputSubmitted);

        dialoguePanel.SetActive(false);
    }


    private void Update()
    {
        if (!isDialogueOpen || isClosing)
            return;

        // F6 ile kamera ve karakter hareketini ac.
        if (!movementReleased &&
            Keyboard.current != null &&
            Keyboard.current.f6Key.wasPressedThisFrame)
        {
            Debug.Log("F6 TUSU ALGILANDI!");

            OnTabMovementRequested();
        }

        if (!movementReleased)
            return;

        if (playerController == null || guardTransform == null)
            return;

        Vector3 playerPosition =
            playerController.transform.position;

        Vector3 guardPosition =
            guardTransform.position;

        playerPosition.y = 0f;
        guardPosition.y = 0f;

        float distance = Vector3.Distance(
            playerPosition,
            guardPosition
        );

        // Muhafizdan uzaklasinca konusma kapanir.
        if (distance > exitDistance)
        {
            CloseDialogue();
        }
    }


    public void OpenDialogue()
    {
        if (isDialogueOpen || isClosing)
            return;

        dialogueVersion++;

        isDialogueOpen = true;
        movementReleased = false;

        dialoguePanel.SetActive(true);

        // Karakterin hareketini durdur.
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }

        // Kamerayi sabitle.
        if (cameraFollow != null)
        {
            cameraFollow.SetDialogueActive(true);
        }

        // Mesaj yazma alanini aktif et.
        playerInput.ActivateInputField();
    }

    private void OnTabMovementRequested()
    {
        if (!isDialogueOpen || isClosing || movementReleased)
            return;

        movementReleased = true;

        // Mesaj yazma alanini kapat.
        playerInput.DeactivateInputField();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Karakter hareketini ac.
        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.SetMovementEnabled(true);
        }

        // Kamerayi tekrar aktif et.
        if (cameraFollow != null)
        {
            cameraFollow.enabled = true;
            cameraFollow.SetDialogueActive(false);
        }

        Debug.Log("F6: Kamera ve karakter hareketi acildi.");
    }


    public void CloseDialogue()
    {
        if (!isDialogueOpen || isClosing)
            return;

        isClosing = true;

        StartCoroutine(CloseDialogueNextFrame());
    }

    private IEnumerator CloseDialogueNextFrame()
    {
        yield return null;

        dialogueVersion++;

        isDialogueOpen = false;
        movementReleased = false;
        isClosing = false;

        playerInput.DeactivateInputField();

        dialoguePanel.SetActive(false);

        // Hareketi geri ac.
        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }

        // Normal kamera takibine don.
        if (cameraFollow != null)
        {
            cameraFollow.SetDialogueActive(false);
        }
    }

    private void OnInputSubmitted(string text)
    {
        OnSendButtonClicked();
    }

    private async void OnSendButtonClicked()
    {
        if (isWaitingForResponse || !isDialogueOpen || isClosing)
            return;

        string message = playerInput.text.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            guardResponseText.text = "Lütfen bir mesaj yaz";
            return;
        }

        isWaitingForResponse = true;
        sendButton.interactable = false;
        playerInput.interactable = false;

        guardResponseText.text = "Düþünüyor...";

        int requestVersion = dialogueVersion;

        try
        {
            GuardLlmResponse response =
                await guardController.SendMessageAsync(message);

            // Diyalog kapatildiysa eski yaniti gosterme.
            if (!isDialogueOpen ||
                requestVersion != dialogueVersion)
            {
                return;
            }

            if (response == null)
            {
                guardResponseText.text =
                    "Muhafýz geçerli bir yanýt üretemedi.";
                return;
            }

            guardResponseText.text = response.reply;
            playerInput.text = "";
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            Debug.LogError(
                "LLM isteði zaman aþýmýna uðradý."
            );

            if (isDialogueOpen &&
                requestVersion == dialogueVersion)
            {
                guardResponseText.text =
                    "Muhafýzýn yanýt vermesi çok uzun sürdü. " +
                    "Lütfen tekrar dene.";
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogError(
                $"LLM isteði sýrasýnda hata oluþtu: {exception.Message}"
            );

            if (isDialogueOpen &&
                requestVersion == dialogueVersion)
            {
                guardResponseText.text =
                    "Muhafýza þu anda ulaþýlamýyor. " +
                    "Lütfen tekrar dene.";
            }
        }
        finally
        {
            isWaitingForResponse = false;

            sendButton.interactable = true;
            playerInput.interactable = true;

            if (isDialogueOpen &&
                !isClosing &&
                !movementReleased &&
                requestVersion == dialogueVersion)
            {
                playerInput.ActivateInputField();
            }
        }
    }
}
