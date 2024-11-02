using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NorebaseLikeFeature.Application.Interfaces.IRepositories;
using NorebaseLikeFeature.Application.Interfaces.IServices;
using NorebaseLikeFeature.Application.Middlewares;
using NorebaseLikeFeature.Application.Services;
using NorebaseLikeFeature.Domain.RateLimiter;
using NorebaseLikeFeature.Domain.User;
using NorebaseLikeFeature.Persistence.Context;
using NorebaseLikeFeature.Persistence.Repositories;
using StackExchange.Redis;
using System.Collections.Concurrent;

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

// Register the ConcurrentDictionary as a singleton
builder.Services.AddSingleton<ConcurrentDictionary<string, RateLimitInfo>>();

// Register the like rate limiter
builder.Services.AddSingleton<ILikeRateLimiter, LikeRateLimiter>();

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

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

builder.Services.AddCustomRateLimiterServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCustomRateLimiter(maxRequests: 100, resetPeriod: TimeSpan.FromMinutes(1));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();