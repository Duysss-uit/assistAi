using AiAgent;
using AiAgent.Tools;
DotNetEnv.Env.Load();
var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
var baseUrl = Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL") 
              ?? "https://openrouter.ai/api/v1";
if (string.IsNullOrEmpty(apiKey))
    throw new InvalidOperationException("OPENROUTER_API_KEY is not set");
string? prompt = null;
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "-p" && i + 1 < args.Length)
    {
        prompt = args[i + 1];
        break;
    }
}
if (string.IsNullOrEmpty(prompt))
{
    Console.WriteLine("Usage: dotnet run -- -p \"your prompt here\"");
    return;
}
new ReadTool();
new WriteTool();
new BashTool();
new WebSearchTool();
new WebScraperTool();
var agent = new AgentLoop(apiKey, baseUrl);
await agent.RunAsync(prompt);
