using System.Text.Json;
using System.Text.Json.Nodes;

namespace AiAgent.Tools;

public abstract class Tool
{
    public string Name { get; }
    public string Description { get; }
    public JsonObject Parameters { get; }

    private static readonly Dictionary<string, Tool> _registry = new();

    protected Tool(string name, string description, JsonObject parameters)
    {
        Name = name;
        Description = description;
        Parameters = parameters;
        _registry[name] = this;
    }
    public abstract string Run(Dictionary<string, string> arguments);

    public JsonObject ToOpenAiFormat()
    {
        return new JsonObject
        {
            ["type"] = "function",
            ["function"] = new JsonObject
            {
                ["name"] = Name,
                ["description"] = Description,
                ["parameters"] = JsonNode.Parse(Parameters.ToJsonString())
            }
        };
    }
    public static string ExecuteToolCall(string toolName, Dictionary<string, string> arguments)
    {
        if (!_registry.ContainsKey(toolName))
            throw new InvalidOperationException($"Unknown tool: {toolName}");

        return _registry[toolName].Run(arguments);
    }
    public static JsonArray GetAllToolSchemas()
    {
        var array = new JsonArray();
        foreach (var tool in _registry.Values)
        {
            array.Add(tool.ToOpenAiFormat());
        }
        return array;
    }
}
