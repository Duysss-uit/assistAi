using System.Text.Json.Nodes;
namespace AiAgent.Tools;
public class WriteTool : Tool
{
    public WriteTool() : base(
        name: "Write",
        description: "Write content to a file",
        parameters: new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["file_path"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The path of the file to write to"
                },
                ["content"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The content to write to the file"
                }
            },
            ["required"] = new JsonArray { "file_path", "content" }
        })
    { }

    public override string Run(Dictionary<string, string> arguments)
    {
        var filePath = arguments["file_path"];
        var content = arguments["content"];
        File.WriteAllText(filePath, content);
        return "OK";
    }
}
