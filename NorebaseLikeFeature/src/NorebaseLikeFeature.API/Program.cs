using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NorebaseLikeFeature.API.Configurations;
using NorebaseLikeFeature.Application.Interfaces.IRepositories;
using NorebaseLikeFeature.Application.Interfaces.IServices;
using NorebaseLikeFeature.Application.Middlewares;
using NorebaseLikeFeature.Application.Services;
using NorebaseLikeFeature.Common.Config;
using NorebaseLikeFeature.Domain.RateLimiter;
using NorebaseLikeFeature.Domain.User;
using NorebaseLikeFeature.Persistence.Context;
using NorebaseLikeFeature.Persistence.Repositories;
using StackExchange.Redis;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddSwagger();



// Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("norebaseConnection")));

// Register AuthSettings with configuration
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("AuthSettings"));

/// Redis configuration
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var redisConnection = builder.Configuration.GetConnectionString("Redis");

    try
    {
        var multiplexer = ConnectionMultiplexer.Connect(redisConnection);

        multiplexer.ConnectionFailed += (sender, args) =>
        {
            logger.LogError("Redis connection failed. {FailureType}: {Exception}",
                args.FailureType,
                args.Exception?.Message);
        };

        multiplexer.ConnectionRestored += (sender, args) =>
        {
            logger.LogInformation("Redis connection restored.");
        };

        multiplexer.ErrorMessage += (sender, args) =>
        {
            logger.LogWarning("Redis error: {Message}", args.Message);
        };

        return multiplexer;
    }
    catch (RedisConnectionException ex)
    {
        logger.LogError(ex, "Failed to connect to Redis server during startup. Application will continue without caching.");

        return ConnectionMultiplexer.Connect("localhost", config =>
        {
            config.AbortOnConnectFail = false;
            config.ClientName = "Fallback";
        });
    }
});

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
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LikeFeatureAPI v1"));
}
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseCustomRateLimiter(maxRequests: 100, resetPeriod: TimeSpan.FromMinutes(1));
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();