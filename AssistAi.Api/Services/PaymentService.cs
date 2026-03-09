using Stripe;
using Stripe.Checkout;
using Microsoft.EntityFrameworkCore;
using AssistAi.Api.Data;
using AssistAi.Api.Models;

namespace AssistAi.Api.Services;

public class PaymentService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    public PaymentService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }
    public async Task<Session> CreateCheckoutSessionAsync(int userId, string email)
    {
        var domain = "http://localhost:5000";
        var priceId = _config["Stripe:ProPlanPriceId"] ?? _config["Stripe:ProPriceId"];
        if (string.IsNullOrWhiteSpace(priceId))
        {
            throw new InvalidOperationException("Stripe price id is missing. Configure Stripe:ProPlanPriceId or Stripe:ProPriceId.");
        }

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            CustomerEmail = email,
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                },
            },
            SuccessUrl = domain + "/pricing.html?success=true",
            CancelUrl = domain + "/pricing.html?canceled=true",
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId.ToString() },
                { "plan", "pro" },
            },
        };
        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session;
    }
    public async Task HandleCheckoutSessionCompletedAsync(string json, string stripeSignature)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _config["Stripe:WebhookSecret"]);
        if (stripeEvent.Type != "checkout.session.completed")
        {
            return;
        }

        var checkoutSession = stripeEvent.Data.Object as Stripe.Checkout.Session;
        var userId = int.Parse(checkoutSession!.Metadata["userId"]);
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.Tier = "Pro";
            var payment = new Payment
            {
                UserId = userId,
                StripeCustomerId = checkoutSession.CustomerId ?? "",
                StripeSubscriptionId = checkoutSession.SubscriptionId ?? "",
                StripeSubscriptionStatus = "active",
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }
    }
}
