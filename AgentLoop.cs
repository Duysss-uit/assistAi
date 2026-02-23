using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AiAgent.Tools;

namespace AiAgent;
public class AgentLoop
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _model;
    public AgentLoop(string apiKey, string baseUrl, string model = "arcee-ai/trinity-large-preview:free")
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl;
        _model = model;
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
