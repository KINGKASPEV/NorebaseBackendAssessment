using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NorebaseLikeFeature.Domain.RateLimiter;
using System.Collections.Concurrent;

namespace NorebaseLikeFeature.Application.Middlewares
{
    public class RateLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConcurrentDictionary<string, RateLimitInfo> _requestCounts = new();
        private readonly int _maxRequests;
        private readonly TimeSpan _resetPeriod;
        private readonly ILogger<RateLimiterMiddleware> _logger;

        public RateLimiterMiddleware(
            RequestDelegate next,
            int maxRequests,
            TimeSpan resetPeriod,
            ILogger<RateLimiterMiddleware> logger)
        {
            _next = next;
            _maxRequests = maxRequests;
            _resetPeriod = resetPeriod;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = GetClientIp(context);
            var currentTime = DateTime.UtcNow;

            var rateLimitInfo = _requestCounts.GetOrAdd(ipAddress, _ => new RateLimitInfo { LastResetTime = currentTime });

            if ((currentTime - rateLimitInfo.LastResetTime) >= _resetPeriod)
            {
                rateLimitInfo.RequestCount = 0;
                rateLimitInfo.LastResetTime = currentTime;
            }

            if (rateLimitInfo.RequestCount >= _maxRequests)
            {
                _logger.LogWarning($"Rate limit exceeded for IP: {ipAddress}. Requests: {rateLimitInfo.RequestCount}");
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many requests. Please try again later.");
                return;
            }

            rateLimitInfo.RequestCount++;

            await _next(context);
        }

        public static string GetClientIp(HttpContext context)
        {
            return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                   ?? context.Connection.RemoteIpAddress?.ToString()
                   ?? "Unknown";
        }
    }
}
