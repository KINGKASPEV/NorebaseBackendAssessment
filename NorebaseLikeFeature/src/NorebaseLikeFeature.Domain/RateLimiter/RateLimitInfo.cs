namespace NorebaseLikeFeature.Domain.RateLimiter
{
    public class RateLimitInfo
    {
        public int RequestCount { get; set; }
        public DateTime LastResetTime { get; set; }
    }
}
