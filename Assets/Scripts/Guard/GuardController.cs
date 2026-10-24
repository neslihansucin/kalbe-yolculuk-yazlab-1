using System.Threading.Tasks;
using UnityEngine;

public class GuardController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GuardActionHandler actionHandler;

    private ILlmClient llmClient;
    private readonly GuardPromptBuilder promptBuilder = new GuardPromptBuilder();
    private readonly GuardResponseValidator responseValidator = new GuardResponseValidator();

    public void Initialize(ILlmClient llmClient)
    {
        this.llmClient = llmClient;
    }

    public async Task<GuardLlmResponse> SendMessageAsync(string message)
    {
        string prompt = promptBuilder.BuildPrompt(message, gameManager.State.HasMemoryOrb, gameManager.State.GuardHintLevel);
        
        string modelResponse = await llmClient.SendPromptAsync(prompt);

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
        $"hasMemoryOrb={gameManager.State.HasMemoryOrb}, " +
        $"hintLevel={gameManager.State.GuardHintLevel}, " +
        $"reply={guardResponse.reply}"
        );

        bool isValid = responseValidator.IsValid(guardResponse, gameManager.State.HasMemoryOrb);

        if (!isValid)
        {
            Debug.LogError("[Guard Validator] LLM cevabý geçersiz.");
            return null;
        }

        if (!GuardActionParser.TryParse(
        guardResponse.action,
        out GuardAction action))
        {
            Debug.LogError(
                $"[Guard Action] Bilinmeyen action: {guardResponse.action}"
            );

            return null;
        }

        if (!GuardIntentParser.TryParse(
        guardResponse.intent,
        out GuardIntent intent))
        {
            Debug.LogError(
                $"[Guard Intent] Bilinmeyen intent: {guardResponse.intent}"
            );

            return null;
        }

        if (intent == GuardIntent.Heal &&
            action == GuardAction.RequestMoreInfo &&
            !gameManager.State.HasMemoryOrb)
        {
            gameManager.State.IncreaseGuardHintLevel();
        }

        actionHandler.HandleAction(action);

        return guardResponse;
    }
}