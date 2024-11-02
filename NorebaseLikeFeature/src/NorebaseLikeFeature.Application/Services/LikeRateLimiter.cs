using Microsoft.Extensions.Logging;
using NorebaseLikeFeature.Application.Interfaces.IServices;
using NorebaseLikeFeature.Domain.RateLimiter;
using System.Collections.Concurrent;

namespace NorebaseLikeFeature.Application.Services
{
    public class LikeRateLimiter : ILikeRateLimiter
    {
        private readonly ConcurrentDictionary<string, RateLimitInfo> _likeLimitStore;
        private readonly ILogger<LikeRateLimiter> _logger;
        private const int MAX_LIKES_PER_PERIOD = 10;
        private static readonly TimeSpan RESET_PERIOD = TimeSpan.FromMinutes(5);

        public LikeRateLimiter(
            ConcurrentDictionary<string, RateLimitInfo> likeLimitStore,
            ILogger<LikeRateLimiter> logger)
        {
            _likeLimitStore = likeLimitStore;
            _logger = logger;
        }

        public async Task<bool> CanPerformLikeActionAsync(string userId)
        {
            await Task.Delay(0);

            var key = $"like:{userId}";
            var currentTime = DateTime.UtcNow;

            var rateLimitInfo = _likeLimitStore.GetOrAdd(key, _ => new RateLimitInfo
            {
                LastResetTime = currentTime,
                RequestCount = 0
            });

            if ((currentTime - rateLimitInfo.LastResetTime) >= RESET_PERIOD)
            {
                rateLimitInfo.RequestCount = 0;
                rateLimitInfo.LastResetTime = currentTime;
            }

            if (rateLimitInfo.RequestCount >= MAX_LIKES_PER_PERIOD)
            {
                _logger.LogWarning($"Like rate limit exceeded for User: {userId}");
                return false;
            }

            rateLimitInfo.RequestCount++;
            return true;
        }
    }
}
