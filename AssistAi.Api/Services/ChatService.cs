namespace AssistAi.Api.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
public class ChatService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    public ChatService(IConfiguration config)
    {
        _config = config;
        _apiKey = _config["OpenRouter:ApiKey"]!;
        _baseUrl = _config["OpenRouter:BaseUrl"]!;
        _httpClient = new HttpClient();
    }
    public async Task<string> RunAsync(string model, string prompt)
    {
        var messages = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "user",
                ["content"] = prompt
            }
        };
        var requestBody = new JsonObject
        {
            ["model"] = model,
            ["messages"] = JsonNode.Parse(messages.ToJsonString()),
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions")
        {
            Content = new StringContent(
                requestBody.ToJsonString(),
                Encoding.UTF8,
                "application/json"
            )
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"API Error ({response.StatusCode}): {responseBody}");
            throw new InvalidOperationException($"API returned {response.StatusCode}");
        }
        var json = JsonNode.Parse(responseBody)!;
        var choices = json["choices"]?.AsArray();
        if (choices == null || choices.Count == 0)
        {
            Console.Error.WriteLine($"Unexpected response (no choices): {responseBody}");
            throw new InvalidOperationException("No choices in response");
        }
        var message = choices[0]!["message"]!;
        var responseContent = message["content"]?.GetValue<string>() ?? "";
        return responseContent;
    }
}
