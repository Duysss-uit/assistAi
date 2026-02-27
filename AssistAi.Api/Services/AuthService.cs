using Microsoft.EntityFrameworkCore;
using AssistAi.Api.Data;
using AssistAi.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace AssistAi.Api.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _config;
    public AuthService(AppDbContext context, ILogger<AuthService> logger, IConfiguration config)
    {
        _context = context;
        _logger = logger;
        _config = config;
    }
    public string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("Tier", user.Tier)
        };
        var secretKey = _config["Jwt:Key"] ?? "DayLaMotCaiKeyBiMatSieuDaiVaSieuBaoMatChoAssistAiThemNhieuChuVaoDay";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(30),
            SigningCredentials = creds
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    public async Task<User?> RegisterAsync(string username, string email, string password)
    {
        bool userExists = await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
        if (userExists)
        {
            return null;
        }
        var user = new User { Username = username, Email = email };
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }
    public async Task<string?> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }
        return GenerateJwtToken(user);
    }
}