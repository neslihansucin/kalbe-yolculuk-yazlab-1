using System.Threading.Tasks;
using UnityEngine;

public class GuardController : MonoBehaviour
{
    [SerializeField] private bool testHasMemoryOrb = false;
    [SerializeField] private int testHintLevel = 0;  // sadece test icin

    private readonly OllamaClient ollamaClient = new OllamaClient();
    private readonly GuardPromptBuilder propmBuilder = new GuardPromptBuilder();
    private readonly GuardResponseValidator responseValidator = new GuardResponseValidator();

    public async Task<GuardLlmResponse> SendMessageAsync(string message)
    {
        string propmpt = propmBuilder.BuildPrompt(message, testHasMemoryOrb, testHintLevel);
        
        string modelResponse = await ollamaClient.SendPromptAsync(propmpt);

        Debug.Log($"[Guard Raw] {modelResponse}");

        GuardLlmResponse guardResponse = JsonUtility.FromJson<GuardLlmResponse>(modelResponse);

        Debug.Log(
        $"[Guard LLM] intent={guardResponse.intent}, " +
        $"action={guardResponse.action}, " +
        $"hasMemoryOrb={testHasMemoryOrb}, " +
        $"hintLevel={testHintLevel}, " +
        $"reply={guardResponse.reply}"
        );

        bool isValid = responseValidator.IsValid(guardResponse, testHasMemoryOrb);

        if (!isValid)
        {
            Debug.LogError("[Guard Validator] LLM cevabý geçersiz.");
            return null;
        }

        return guardResponse;
    }
}