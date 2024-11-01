using NorebaseLikeFeature.Domain.RateLimiter;
using System.Collections.Concurrent;

namespace NorebaseLikeFeature.Application.Middlewares
{
    public static class WebApplicationRateLimiterExtensions
    {
        public static WebApplication UseCustomRateLimiter(
            this WebApplication app,
            int maxRequests = 10,
            TimeSpan? resetPeriod = null)
        {
            resetPeriod ??= TimeSpan.FromSeconds(1);

            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<RateLimiterMiddleware>>();
                var requestCounts = context.RequestServices.GetRequiredService<ConcurrentDictionary<string, RateLimitInfo>>();

                await RateLimiterMiddlewareExtensions.HandleRateLimiting(
                    context,
                    maxRequests,
                    resetPeriod.Value,
                    logger,
                    requestCounts);

                await next();
            });

            return app;
        }
    }
}
