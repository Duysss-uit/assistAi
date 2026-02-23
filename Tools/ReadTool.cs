using System.Text.Json.Nodes;

namespace AiAgent.Tools;

public class ReadTool : Tool
{
    public ReadTool() : base(
        name: "Read",
        description: "Read and return the content of a file",
        parameters: new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["file_path"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The path to the file to read"
                }
            },
            ["required"] = new JsonArray { "file_path" }
        })
    { }
    public override string Run(Dictionary<string, string> arguments)
    {
        var filePath = arguments["file_path"];
        return File.ReadAllText(filePath);
    }
}
