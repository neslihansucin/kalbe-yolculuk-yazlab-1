using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_Text guardResponseText;
    [SerializeField] private GuardController guardController;
    private void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClicked);
        playerInput.onSubmit.AddListener(OnInputSubmitted);
    }

    private void OnInputSubmitted(string text)
    {
        OnSendButtonClicked();
    }

    private async void OnSendButtonClicked()
    {
        string message = playerInput.text.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            guardResponseText.text = "Lütfen bir mesaj yaz";
            return;
        }

        guardResponseText.text = "Düþünüyor...";

        GuardLlmResponse response = await guardController.SendMessageAsync(message);

        if (response == null)
        {
            guardResponseText.text = "Muhafýz geçerli bir yanýt üretemedi.";
            return;
        }

        guardResponseText.text = response.reply;
        playerInput.text = "";
        playerInput.ActivateInputField();
    }
}
