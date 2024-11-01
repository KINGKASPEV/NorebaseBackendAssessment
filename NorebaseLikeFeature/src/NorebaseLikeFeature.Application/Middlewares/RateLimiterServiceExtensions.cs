using Microsoft.Extensions.DependencyInjection;
using NorebaseLikeFeature.Domain.RateLimiter;
using System.Collections.Concurrent;

namespace NorebaseLikeFeature.Application.Middlewares
{
    public static class RateLimiterServiceExtensions
    {
        public static IServiceCollection AddCustomRateLimiterServices(this IServiceCollection services)
        {
            services.AddSingleton<ConcurrentDictionary<string, RateLimitInfo>>();
            return services;
        }
    }
}
