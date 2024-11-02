using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NorebaseLikeFeature.Application.DTOs.Response;
using NorebaseLikeFeature.Application.Interfaces.IRepositories;
using NorebaseLikeFeature.Application.Interfaces.IServices;
using NorebaseLikeFeature.Common.Responses;
using NorebaseLikeFeature.Common.Utilities;
using NorebaseLikeFeature.Domain.Article;
using NorebaseLikeFeature.Domain.User;
using StackExchange.Redis;

namespace NorebaseLikeFeature.Application.Services
{
    public class LikeService : ILikeService
    {
        private readonly IArticleLikeRepository _repository;
        private readonly IConnectionMultiplexer _redis;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILikeRateLimiter _likeRateLimiter;
        private readonly IArticleRepository _articleRepository;
        private readonly ILogger<LikeService> _logger;

        public LikeService(
            IArticleLikeRepository repository,
            IConnectionMultiplexer redis,
            UserManager<ApplicationUser> userManager,
            ILikeRateLimiter likeRateLimiter,
             IArticleRepository articleRepository,
            ILogger<LikeService> logger)
        {
            _repository = repository;
            _redis = redis;
            _userManager = userManager;
            _likeRateLimiter = likeRateLimiter;
            _articleRepository = articleRepository;
            _logger = logger;
        }

        public async Task<Response<LikeResponse>> GetLikesAsync(string articleId, string userId)
        {
            var response = new Response<LikeResponse>();
            try
            {
                if (string.IsNullOrWhiteSpace(articleId) || string.IsNullOrWhiteSpace(userId))
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = Constants.InvalidLikeDataMessage;
                    return response;
                }

                var article = await _articleRepository.GetByIdAsync(articleId);
                if (article is null)
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = Constants.ArticleNotFoundMessage;
                    return response;
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = Constants.UserNotFoundMessage;
                    return response;
                }

                var db = _redis.GetDatabase();
                var cacheKey = $"article:{articleId}:likes";
                var cachedCount = await db.StringGetAsync(cacheKey);

                int totalLikes;
                try
                {
                    if (!cachedCount.HasValue)
                    {
                        totalLikes = await _repository.GetLikeCountAsync(articleId);
                        await db.StringSetAsync(cacheKey, totalLikes, TimeSpan.FromMinutes(5));
                    }
                    else
                    {
                        totalLikes = (int)cachedCount;
                    }
                }
                catch (RedisConnectionException ex)
                {
                    _logger.LogError(ex, "Redis server is unavailable. Falling back to database for like count.");
                    totalLikes = await _repository.GetLikeCountAsync(articleId);
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
                if (string.IsNullOrWhiteSpace(articleId) || string.IsNullOrWhiteSpace(userId))
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = Constants.InvalidLikeDataMessage;
                    return response;
                }

                if (!await _likeRateLimiter.CanPerformLikeActionAsync(userId))
                {
                    response.StatusCode = StatusCodes.Status429TooManyRequests;
                    response.Message = Constants.MaximumNumberOfLikeReached;
                    return response;
                }

                var article = await _articleRepository.GetByIdAsync(articleId);
                if (article is null)
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = Constants.ArticleNotFoundMessage;
                    return response;
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
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
                int totalLikes;
                try
                {
                    var db = _redis.GetDatabase();
                    var cacheKey = $"article:{articleId}:likes";
                    totalLikes = await _repository.GetLikeCountAsync(articleId);
                    await db.StringSetAsync(cacheKey, totalLikes, TimeSpan.FromMinutes(5));
                }
                catch (RedisConnectionException ex)
                {
                    _logger.LogError(ex, "Redis server is unavailable. Falling back to database for like count.");
                    totalLikes = await _repository.GetLikeCountAsync(articleId);
                }

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
