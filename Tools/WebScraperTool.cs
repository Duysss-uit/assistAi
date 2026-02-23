using System.Text;                    
using System.Text.Json.Nodes;         
using HtmlAgilityPack;   
namespace AiAgent.Tools; 
public class WebScraperTool : Tool
{
    public WebScraperTool() : base(
        name: "WebScraper",
        description: "Scrape a website for content",
        parameters: new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["url"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The URL to scrape"
                }
            },
            ["required"] = new JsonArray { "url" }
        })
    { }
    private string FormatWebScraperResults(HtmlDocument doc)
    {
        var tagsToRemove = new [] { "script", "style", "nav", "footer", "header", "aside", "form", "button", "input", "select", "textarea" };
        foreach (var tag in tagsToRemove)
        {
            var elements = doc.DocumentNode.SelectNodes($"//{tag}");
            if(elements != null)
            {
                foreach (var element in elements)
                {
                    element.Remove();
                }
            }
        }
        var body = doc.DocumentNode.SelectSingleNode("//body");
        if(body == null)
            return "No results found";
        var text = body.InnerText.Trim();
        if (text.Length > 5000)
            text = text.Substring(0, 5000) + "\n[... Content truncated]";
        return text;
    }
    public override string Run(Dictionary<string, string> arguments)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        );
        var url = arguments["url"];
        var html = httpClient.GetStringAsync(url).Result;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return FormatWebScraperResults(doc);
    }
}