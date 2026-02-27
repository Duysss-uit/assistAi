namespace AssistAi.Api.Models;

public class UsageLog{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ModelId { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
}