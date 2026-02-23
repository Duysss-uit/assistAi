using System.Diagnostics;
using System.Text.Json.Nodes;  
namespace AiAgent.Tools;
public class OpenBrowserTool:Tool{
    public OpenBrowserTool() : base(
        name: "OpenBrowser",
        description: "Open a browser to the specified URL",
        parameters: new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["url"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The URL to open"
                }
            },
            ["required"] = new JsonArray { "url" }
        })
    { }
    private void OpenBrowser(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
    public override string Run(Dictionary<string, string> arguments)
    {
        var url = arguments["url"];
        OpenBrowser(url);
        return $"Opening browser to {url}";
    }
}