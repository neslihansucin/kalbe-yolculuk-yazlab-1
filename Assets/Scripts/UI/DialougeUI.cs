using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_Text guardResponseText;
    [SerializeField] private GuardController guardController;
    [SerializeField] private GameObject dialoguePanel;

    private bool isWaitingForResponse = false;

    private void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClicked);
        playerInput.onSubmit.AddListener(OnInputSubmitted);
        dialoguePanel.SetActive(false);
    }

    public void OpenDialogue()
    {
        dialoguePanel.SetActive(true);
        playerInput.ActivateInputField();
    }

    public void CloseDialogue()
    {
        StartCoroutine(CloseDialogueNextFrame());
    }

    private IEnumerator CloseDialogueNextFrame()
    {
        yield return null;
        dialoguePanel.SetActive(false);
    }

    private void OnInputSubmitted(string text)
    {
        OnSendButtonClicked();
    }

    private async void OnSendButtonClicked()
    {
        if (isWaitingForResponse)
        {
            return;
        }

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


        try
        {
            GuardLlmResponse response = await guardController.SendMessageAsync(message);

            if (response == null)
            {
                guardResponseText.text = "Muhafýz geçerli bir yanýt üretemedi.";
                return;
            }

            guardResponseText.text = response.reply;
            playerInput.text = "";
        }
        catch(System.Threading.Tasks.TaskCanceledException)
        {
            Debug.LogError("LLM isteði zaman aþýmýna uðradý.");

            guardResponseText.text = " Muhafýzýn yanýt vermesi çok uzun sürdü. Lütfen tekrar dene.";
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"LLM isteði sýrasýnda hata oluþtu : {exception.Message}");

            guardResponseText.text = "Muhafýza þu anda ulaþýlamýyor. Lütfen tekrar dene.";
        }
        finally
        {
            isWaitingForResponse=false;
            sendButton.interactable = true;
            playerInput.interactable = true;
            playerInput.ActivateInputField();

        }
    }
}
