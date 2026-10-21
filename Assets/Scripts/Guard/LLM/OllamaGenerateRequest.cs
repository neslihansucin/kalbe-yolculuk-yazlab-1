

[System.Serializable]
public class OllamaGenerateRequest
{
    public string model;
    public string prompt;
    public bool stream;
    public GuardResponseSchema format;
}
