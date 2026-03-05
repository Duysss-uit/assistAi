namespace AssistAi.Api.Services;

using Microsoft.EntityFrameworkCore;
using AssistAi.Api.Data;
using AssistAi.Api.Models;
public class UsageService(AppDbContext context)
{
    private readonly AppDbContext _context = context;
    public async Task LogUsageAsync(int userId, string modelId)
    {
        var log = new UsageLog
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            ModelId = modelId
        };
        _context.UsageLogs.Add(log);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> HasReachedLimitAsync(int userId, string tier)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var count = await _context.UsageLogs.CountAsync(u => u.UserId == userId && u.Timestamp >= startOfMonth);
        var limit = tier switch
        {
            "free" => 10,
            "pro" => 100,
            _ => 1000
        };
        if (count >= limit)
        {
            return true;
        }
        return false;
    }
}