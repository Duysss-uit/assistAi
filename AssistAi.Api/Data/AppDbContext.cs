using Microsoft.EntityFrameworkCore;
using AssistAi.Api.Models;

namespace AssistAi.Api.Data;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<UsageLog> UsageLogs { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}