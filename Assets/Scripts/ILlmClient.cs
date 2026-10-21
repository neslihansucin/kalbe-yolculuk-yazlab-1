using System.Threading.Tasks;

public interface ILlmClient
{
    Task<string> SendPromptAsync(string prompt);
}