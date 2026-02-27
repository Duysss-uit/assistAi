using AssistAi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssistAi.Api.Controllers;
//Dto 
public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";

}
public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
//API 
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
        if (user == null)
        {
            return BadRequest(new { Message = "Email of Username already exists" });
        }
        return Ok(new { Message = "User registered successfully" });
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request.Username, request.Password);
        if (token == null)
        {
            return BadRequest(new { Message = "Username or password is incorrect" });
        }
        return Ok(new { Token = token });
    }
}
