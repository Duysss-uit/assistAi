using AssistAi.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AssistAi.Api.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddControllers();

// CORS — cho phép frontend gọi API từ cùng origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var secretKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT secret key 'Jwt:Key' is not configured.");
var key = Encoding.ASCII.GetBytes(secretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsageService>();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseCors("AllowAll");       // Cho phép CORS
// app.UseHttpsRedirection(); // Tạm bỏ — gây lỗi "Failed to fetch" khi chạy HTTP ở development

// Phục vụ file tĩnh từ wwwroot/ (HTML, CSS, JS)
app.UseDefaultFiles();         // Tự động phục vụ index.html khi truy cập /
app.UseStaticFiles();          // Phục vụ các file tĩnh (css, js, images,...)

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// SPA fallback — nếu không tìm thấy route API, trả về index.html
app.MapFallbackToFile("index.html");

app.Run();

