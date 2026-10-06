using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OllamaClient
{
    private static readonly HttpClient httpClient = new HttpClient();

    private const string apiUrl = "http://localhost:11434/api/generate";
    private const string modelName = "gemma3:4b";

    private GuardResponseSchema CreateResponseSchema()
    {
        return new GuardResponseSchema
        {
            type = "object",

            properties = new GuardResponseProperties
            {
                reply = new GuardStringProperty
                {
                    type = "string"
                },

                intent = new GuardIntentProperty
                {
                    type = "string",
                    @enum = new[] { "heal", "harmful", "unclear" }
                },

                action = new GuardActionProperty
                {
                    type = "string",
                    @enum = new[]
                    {
                    "allow_entry",
                    "request_more_info",
                    "deny"
                }
                }
            },

            required = new[]
            {
            "reply",
            "intent",
            "action"
        }
        };
    }

    private string CreateRequestJson(string prompt)
    {
        OllamaGenerateRequest request = new OllamaGenerateRequest
        {
            model = modelName,
            prompt = prompt,
            stream = false,
            format = CreateResponseSchema()
        };


        return JsonUtility.ToJson(request);
    }

    public async Task<string> SendPromptAsync(string prompt)
    {
        string requestJson = CreateRequestJson(prompt);

        using StringContent content = new StringContent(
            requestJson,
            Encoding.UTF8,
            "application/json"
        );

        using HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

        response.EnsureSuccessStatusCode();

        string rawResponse = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Debug.LogError(
                $"Ollama hata verdi. Status: {(int)response.StatusCode} {response.StatusCode}\n" +
                $"Cevap: {rawResponse}"
                );

            return null;
        }

        OllamaGenerateResponse ollamaResponse =
            JsonUtility.FromJson<OllamaGenerateResponse>(rawResponse);

        return ollamaResponse.response;
    }

} 