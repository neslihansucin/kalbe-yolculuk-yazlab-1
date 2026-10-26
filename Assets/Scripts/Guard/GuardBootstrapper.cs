using UnityEngine;

public class GuardBootstrapper : MonoBehaviour
{
    [SerializeField] private GuardController guardController;

    private void Awake()
    {
        ILlmClient llmClient = new OllamaClient();
        guardController.Initialize(llmClient);
    }
}
