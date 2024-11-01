using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NorebaseLikeFeature.Domain.RateLimiter;
using System.Collections.Concurrent;

namespace NorebaseLikeFeature.Application.Middlewares
{
    public static class RateLimiterMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomRateLimiter(
            this IApplicationBuilder builder,
            int maxRequests = 10,
            TimeSpan? resetPeriod = null)
        {
            resetPeriod ??= TimeSpan.FromSeconds(1);

            return builder.UseMiddleware<RateLimiterMiddleware>(
                maxRequests,
                resetPeriod.Value,
                builder.ApplicationServices.GetRequiredService<ILogger<RateLimiterMiddleware>>()
            );
        }

        public static async Task HandleRateLimiting(
            HttpContext context,
            int maxRequests,
            TimeSpan resetPeriod,
            ILogger<RateLimiterMiddleware> logger,
            ConcurrentDictionary<string, RateLimitInfo> requestCounts)
        {
            var ipAddress = RateLimiterMiddleware.GetClientIp(context);
            var currentTime = DateTime.UtcNow;

            var rateLimitInfo = requestCounts.GetOrAdd(ipAddress, _ => new RateLimitInfo
            {
                LastResetTime = currentTime
            });

            if ((currentTime - rateLimitInfo.LastResetTime) >= resetPeriod)
            {
                rateLimitInfo.RequestCount = 0;
                rateLimitInfo.LastResetTime = currentTime;
            }

            if (rateLimitInfo.RequestCount >= maxRequests)
            {
                logger.LogWarning($"Rate limit exceeded for IP: {ipAddress}. Requests: {rateLimitInfo.RequestCount}");
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many requests. Please try again later.");
                return;
            }

            rateLimitInfo.RequestCount++;
        }
    }
}
