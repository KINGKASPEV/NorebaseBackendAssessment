using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using NorebaseLikeFeature.Application.Interfaces.IRepositories;
using NorebaseLikeFeature.Application.Interfaces.IServices;
using NorebaseLikeFeature.Application.Middlewares;
using NorebaseLikeFeature.Application.Services;
using NorebaseLikeFeature.Domain.User;
using NorebaseLikeFeature.Persistence.Context;
using NorebaseLikeFeature.Persistence.Repositories;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("norebaseConnection")));

// Redis configuration
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

// Repository and Service registration
builder.Services.AddScoped<IArticleLikeRepository, ArticleLikeRepository>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IArticleService, ArticleService>();

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(builder.Configuration["AuthSettings:SecretKey"])),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddCheck("Redis", () =>
    {
        try
        {
            using (var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")))
            {
                var db = redis.GetDatabase();
                var pingResult = db.Ping();
                return pingResult != TimeSpan.Zero
                    ? HealthCheckResult.Healthy("Redis is healthy")
                    : HealthCheckResult.Unhealthy("Redis is unhealthy");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Redis connection failed: {ex.Message}");
        }
    });

// Add this before builder.Build()
builder.Services.AddCustomRateLimiterServices();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCustomRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();