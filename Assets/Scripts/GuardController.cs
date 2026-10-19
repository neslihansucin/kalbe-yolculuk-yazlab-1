using System.Threading.Tasks;
using UnityEngine;

public class GuardController : MonoBehaviour
{
    [SerializeField] private bool testHasMemoryOrb = false;
    [SerializeField] private int testHintLevel = 0;  // sadece test icin

    private readonly OllamaClient ollamaClient = new OllamaClient();
    private readonly GuardPromptBuilder promptBuilder = new GuardPromptBuilder();
    private readonly GuardResponseValidator responseValidator = new GuardResponseValidator();

    public async Task<GuardLlmResponse> SendMessageAsync(string message)
    {
        string prompt = promptBuilder.BuildPrompt(message, testHasMemoryOrb, testHintLevel);
        
        string modelResponse = await ollamaClient.SendPromptAsync(prompt);

        if(string.IsNullOrWhiteSpace(modelResponse))
        {
            Debug.LogError("[Guard LLM] Model boþ cevap döndürdü.");
            return null;
        }

        GuardLlmResponse guardResponse;

        try
        {
            guardResponse = JsonUtility.FromJson<GuardLlmResponse>(modelResponse);
        }
        catch(System.Exception exception)
        {
            Debug.LogError($"[Guard LLM] JSON ayrýþtýrýlamadý: {exception.Message}\n" + $"Ham cevap: {modelResponse}");

            return null;
        }

        if (guardResponse == null)
        {
            Debug.LogError("[Guard LLM] JSON nesneye dönüþtürülemedi.");
            return null;
        }

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