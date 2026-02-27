namespace AssistAi.Api.Models;

public class Payment{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StripeCustomerId { get; set; } = string.Empty;
    public string StripeSubscriptionId { get; set; } = string.Empty;
    public string StripeSubscriptionItemId { get; set; } = string.Empty;
    public string StripeSubscriptionStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
}