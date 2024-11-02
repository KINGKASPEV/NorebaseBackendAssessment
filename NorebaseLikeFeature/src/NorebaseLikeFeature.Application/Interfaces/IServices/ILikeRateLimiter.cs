namespace NorebaseLikeFeature.Application.Interfaces.IServices
{
    public interface ILikeRateLimiter
    {
        Task<bool> CanPerformLikeActionAsync(string userId);
    }
}
