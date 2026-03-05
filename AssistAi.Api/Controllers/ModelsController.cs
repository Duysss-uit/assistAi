using AssistAi.Api.Services;
using AssistAi.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace AssistAi.Api.Controllers;

public class ModelRequest
{
    public List<string> ModelId { get; set; } = new List<string>();
}
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModelsController : ControllerBase
{
    public ModelsController()
    {
    }
    [HttpGet]
    public async Task<IActionResult> GetModels()
    {
        var tier = User.FindFirst("Tier")?.Value ?? "Free";
        var models = new List<object>();
        models.Add("stepfun/step-3.5-flash:free");
        models.Add("arcee-ai/trinity-large-preview:free");
        models.Add("upstage/solar-pro-3:free");
        models.Add("qwen/qwen3-vl-30b-a3b-thinking:free");
        models.Add("openai/gpt-oss-120b:free");
        models.Add("openai/gpt-oss-20b:free");
        models.Add("z-ai/glm-4.5-air:free");
        models.Add("mistralai/mistral-small-3.1-24b-instruct:free");
        if (tier == "Pro")
        {
            models.Add("openai/gpt-5-codex");
            models.Add("google/gemini-3.1-pro");
            models.Add("anthropic/claude-4.5-sonnet");
        }
        return Ok(models);
    }
}