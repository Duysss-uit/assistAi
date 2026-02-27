namespace AssistAi.Api.Models;

public class User{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Tier { get; set; } = "Free";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Payment> Payments { get; set; } = new();
    public List<UsageLog> UsageLogs { get; set; } = new();
}