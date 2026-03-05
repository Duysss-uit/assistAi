using AssistAi.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace AssistAi.Api.Controllers;
//dto 
public class chatRequest
{
    public string Model { get; set; } = "";
    public string Prompt { get; set; } = "";
}
//API 
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;
    private readonly UsageService _usageService;
    public ChatController(ChatService chatService, UsageService usageService)
    {
        _chatService = chatService;
        _usageService = usageService;
    }
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] chatRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var tier = User.FindFirst(ClaimTypes.Role)?.Value ?? "free";
        if (await _usageService.HasReachedLimitAsync(userId, tier))
        {
            return StatusCode(429, new { Error = "Usage limit reached" });
        }
        var response = await _chatService.RunAsync(request.Model, request.Prompt);
        await _usageService.LogUsageAsync(userId, request.Model);
        return Ok(new { Response = response });
    }
}