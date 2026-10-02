using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_Text guardResponseText;

    private void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClicked);
    }

    private void OnSendButtonClicked()
    {
        string message = playerInput.text.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            guardResponseText.text = "Lütfen bir mesaj yaz";
            return;
        }

        guardResponseText.text ="Mesaj alýndý :" + message;
    }
}
