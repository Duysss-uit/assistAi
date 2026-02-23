using System.Diagnostics;
using System.Text.Json.Nodes;
namespace AiAgent.Tools;
public class BashTool : Tool
{
    public BashTool() : base(
        name: "Bash",
        description: "Execute a shell command",
        parameters: new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["command"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The command to execute"
                }
            },
            ["required"] = new JsonArray { "command" }
        })
    { }
    public override string Run(Dictionary<string, string> arguments)
    {
        var command = arguments["command"];
        var isWindows = OperatingSystem.IsWindows();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = isWindows ? "cmd.exe" : "/bin/bash",
                Arguments = isWindows ? $"/c {command}" : $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return stdout + stderr;
    }
}
