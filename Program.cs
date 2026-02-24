using AiAgent;
using AiAgent.Tools;
using AiAgent.Models;

// Đăng ký tất cả tools
new ReadTool();
new WriteTool();
new BashTool();
new WebSearchTool();
new WebScraperTool();
new OpenBrowserTool();
void changeModel(Model model){
    model.ModelId = model.ModelChoosing();
}
var model = new Model();
string? prompt = null;
var agent = new AgentLoop(model);
while(true){
    Console.Write("User: ");
    prompt = Console.ReadLine().Trim();
    if(string.IsNullOrEmpty(prompt)){
        continue;
    }
    if(prompt.StartsWith("/")){
        if(prompt == "/model"){
            changeModel(model);
            agent = new AgentLoop(model);
            continue;
        }
        if(prompt == "/clear"){
            Console.Clear();
            continue;
        }
        if(prompt == "/exit"){
            break;
        }
        if(prompt == "/help"){
            Console.WriteLine("| Command  | Description                  | \n|----------|------------------------------|\n| `/model` | Switch to a different model |\n| `/clear` | Clear the console           |\n| `/exit`  | Exit the program            |");
            continue;
        }
    }
    await agent.RunAsync(prompt);
}
