using System.Security.Claims;
using AssistAi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace AssistAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly IConfiguration _config;

    public PaymentController(PaymentService paymentService, IConfiguration config)
    {
        _paymentService = paymentService;
        _config = config;
    }

    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckoutSession()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);

        if (!int.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { Message = "Invalid user claims." });
        }

        var session = await _paymentService.CreateCheckoutSessionAsync(userId, email);
        return Ok(new { Url = session.Url, SessionId = session.Id });
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            if (string.IsNullOrWhiteSpace(signature))
            {
                return BadRequest(new { Message = "Missing Stripe-Signature header." });
            }

            await _paymentService.HandleCheckoutSessionCompletedAsync(payload, signature);
            return Ok();
        }
        catch (StripeException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
    [AllowAnonymous]
    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        var publishableKey = _config["Stripe:PublishableKey"];
        return Ok(new { PublishableKey = publishableKey });
    }
}
