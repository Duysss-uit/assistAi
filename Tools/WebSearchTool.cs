using System.Text;                    
using System.Text.Json.Nodes;         
using HtmlAgilityPack;   
namespace AiAgent.Tools;             
public class WebSearchTool : Tool
{
    public WebSearchTool() : base(
        name: "WebSearch",
        description: "Search the web for information",
        parameters: new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["query"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The search query"
                }
            },
            ["required"] = new JsonArray { "query" }
        })
    { }
    public override string Run(Dictionary<string, string> arguments)
    {
        var query = arguments["query"];
        var encodedQuery = Uri.EscapeDataString(query);
        var url = $"https://html.duckduckgo.com/html/?q={encodedQuery}";
        var webScraperTool = new WebScraperTool();
        return webScraperTool.Run(new Dictionary<string, string> { { "url", url } });
    }
}