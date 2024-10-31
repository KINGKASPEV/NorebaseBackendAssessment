using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NorebaseLikeFeature.Application.DTOs.Response;
using NorebaseLikeFeature.Application.Interfaces.IRepositories;
using NorebaseLikeFeature.Application.Interfaces.IServices;
using NorebaseLikeFeature.Common.Responses;
using NorebaseLikeFeature.Common.Utilities;
using NorebaseLikeFeature.Domain.Article;
using StackExchange.Redis;

namespace NorebaseLikeFeature.Application.Services
{
    public class LikeService : ILikeService
    {
        private readonly IArticleLikeRepository _repository;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<LikeService> _logger;

        public LikeService(
            IArticleLikeRepository repository,
            IConnectionMultiplexer redis,
            ILogger<LikeService> logger)
        {
            _repository = repository;
            _redis = redis;
            _logger = logger;
        }

        public async Task<Response<LikeResponse>> GetLikesAsync(string articleId, string userId)
        {
            var response = new Response<LikeResponse>();
            try
            {
                var db = _redis.GetDatabase();
                var cacheKey = $"article:{articleId}:likes";
                var cachedCount = await db.StringGetAsync(cacheKey);

                int totalLikes;
                if (!cachedCount.HasValue)
                {
                    totalLikes = await _repository.GetLikeCountAsync(articleId);
                    await db.StringSetAsync(cacheKey, totalLikes, TimeSpan.FromMinutes(5));
                }
                else
                {
                    totalLikes = (int)cachedCount;
                }

                var hasUserLiked = await _repository.HasUserLikedAsync(articleId, userId);

                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.SuccessMessage;
                response.Data = new LikeResponse
                {
                    TotalLikes = totalLikes,
                    HasUserLiked = hasUserLiked
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting likes for article {ArticleId}", articleId);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = Constants.DefaultExceptionFriendlyMessage;
                return response;
            }
        }

        public async Task<Response<LikeResponse>> ToggleLikeAsync(string articleId, string userId)
        {
            var response = new Response<LikeResponse>();
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = Constants.UserNotFoundMessage;
                    return response;
                }

                var existingLike = await _repository.GetLikeAsync(articleId, userId);

                if (existingLike is not null)
                {
                    await _repository.RemoveLikeAsync(existingLike);
                }
                else
                {
                    await _repository.AddLikeAsync(new ArticleLike
                    {
                        ArticleId = articleId,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                var db = _redis.GetDatabase();
                var cacheKey = $"article:{articleId}:likes";
                var totalLikes = await _repository.GetLikeCountAsync(articleId);
                await db.StringSetAsync(cacheKey, totalLikes, TimeSpan.FromMinutes(5));

                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.SuccessMessage;
                response.Data = new LikeResponse
                {
                    TotalLikes = totalLikes,
                    HasUserLiked = existingLike is null
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like for article {ArticleId}", articleId);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = Constants.DefaultExceptionFriendlyMessage;
                return response;
            }
        }
    }
}
