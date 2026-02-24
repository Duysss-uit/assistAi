using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AiAgent.Tools;
using AiAgent.Models;

namespace AiAgent;
public class AgentLoop
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _model;

    public AgentLoop(Model model)
    {
        // Kiểm tra các thông tin bắt buộc
        if (string.IsNullOrEmpty(model.ApiKey))
            throw new InvalidOperationException("OPENROUTER_API_KEY is not set");
        if (string.IsNullOrEmpty(model.BaseUrl))
            throw new InvalidOperationException("OPENROUTER_BASE_URL is not set");
        if (string.IsNullOrEmpty(model.ModelId))
            throw new InvalidOperationException("Model is not selected");

        _apiKey = model.ApiKey;
        _baseUrl = model.BaseUrl;
        _model = model.ModelId;
        _httpClient = new HttpClient();
    }

    public async Task RunAsync(string prompt)
    {
        var messages = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "user",
                ["content"] = prompt
            }
        };
        var toolSchemas = Tool.GetAllToolSchemas();

        while (true)
        {
            var requestBody = new JsonObject
            {
                ["model"] = _model,
                ["messages"] = JsonNode.Parse(messages.ToJsonString()),
                ["tools"] = JsonNode.Parse(toolSchemas.ToJsonString()),
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
            var toolCalls = message["tool_calls"]?.AsArray();

            if (toolCalls != null && toolCalls.Count > 0)
            {
                messages.Add(JsonNode.Parse(message.ToJsonString())!);
                foreach (var toolCall in toolCalls)
                {
                    var toolName = toolCall!["function"]!["name"]!.GetValue<string>();
                    var argsJson = toolCall["function"]!["arguments"]!.GetValue<string>();
                    var toolCallId = toolCall["id"]!.GetValue<string>();
                    var args = JsonSerializer.Deserialize<Dictionary<string, string>>(argsJson)!;
                    var result = Tool.ExecuteToolCall(toolName, args);
                    messages.Add(new JsonObject
                    {
                        ["role"] = "tool",
                        ["content"] = result,
                        ["tool_call_id"] = toolCallId
                    });
                }
            }
            else
            {
                var responseContent = message["content"]?.GetValue<string>() ?? "";
                Console.WriteLine(responseContent);
                break;
            }
        }
    }
}
