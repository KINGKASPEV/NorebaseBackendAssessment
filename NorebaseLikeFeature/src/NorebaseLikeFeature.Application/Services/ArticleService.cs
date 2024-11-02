using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NorebaseLikeFeature.Application.DTOs.Article;
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
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _repository;
        private readonly IConnectionMultiplexer _redis;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ArticleService> _logger;

        public ArticleService(
            IArticleRepository repository,
            IConnectionMultiplexer redis,
            UserManager<ApplicationUser> userManager,
            ILogger<ArticleService> logger)
        {
            _repository = repository;
            _redis = redis;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Response<ArticleResponse>> GetArticleAsync(string articleId)
        {
            var response = new Response<ArticleResponse>();
            try
            {
                var cacheKey = $"article:{articleId}";
                Article article = null;

                try
                {
                    var db = _redis.GetDatabase();
                    var cachedArticle = await db.StringGetAsync(cacheKey);

                    if (cachedArticle.HasValue)
                    {
                        article = System.Text.Json.JsonSerializer.Deserialize<Article>(cachedArticle);
                    }
                }
                catch (RedisConnectionException ex)
                {
                    _logger.LogWarning(ex, "Redis connection failed for article {ArticleId}. Falling back to database.", articleId);
                }

                if (article is null)
                {
                    article = await _repository.GetByIdAsync(articleId);

                    if (article is not null)
                    {
                        try
                        {
                            var db = _redis.GetDatabase();
                            await db.StringSetAsync(
                                cacheKey,
                                System.Text.Json.JsonSerializer.Serialize(article),
                                TimeSpan.FromMinutes(5)
                            );
                        }
                        catch (RedisException ex)
                        {
                            _logger.LogWarning(ex, "Failed to cache article {ArticleId}", articleId);
                        }
                    }
                }

                if (article is null)
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = Constants.ArticleNotFoundMessage;
                    return response;
                }

                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.SuccessMessage;
                response.Data = MapToArticleResponse(article);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article {ArticleId}", articleId);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = Constants.DefaultExceptionFriendlyMessage;
                return response;
            }
        }

        public async Task<Response<List<ArticleResponse>>> GetAllArticlesAsync()
        {
            var response = new Response<List<ArticleResponse>>();
            try
            {
                var articles = await _repository.GetAllAsync();

                if (articles is null || !articles.Any())
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = Constants.NoArticlesFoundMessage;
                    response.Data = new List<ArticleResponse>(); 
                    return response;
                }

                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.SuccessMessage;
                response.Data = articles.Select(MapToArticleResponse).ToList();
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all articles");
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = Constants.DefaultExceptionFriendlyMessage;
                return response;
            }
        }

        public async Task<Response<ArticleResponse>> CreateArticleAsync(CreateArticleRequest request, string userId)
        {
            var response = new Response<ArticleResponse>();
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = Constants.InvalidArticleDataMessage;
                    return response;
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = Constants.UserNotFoundMessage;
                    return response;
                }

                var article = new Article
                {
                    Title = request.Title,
                    Content = request.Content,
                    AuthorId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                var createdArticle = await _repository.CreateAsync(article);

                if (createdArticle is null)
                {
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Message = Constants.ArticleCreationFailedMessage;
                    return response;
                }

                response.StatusCode = StatusCodes.Status201Created;
                response.Message = Constants.ArticleCreationSuccessMessage;
                response.Data = MapToArticleResponse(createdArticle);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating article");
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = Constants.DefaultExceptionFriendlyMessage;
                return response;
            }
        }

        public async Task<Response<ArticleResponse>> UpdateArticleAsync(string articleId, UpdateArticleRequest request, string userId)
        {
            var response = new Response<ArticleResponse>();
            try
            {
                var article = await _repository.GetByIdAsync(articleId);
                if (article is null)
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = Constants.ArticleNotFoundMessage;
                    return response;
                }

                if (article.AuthorId != userId)
                {
                    response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Message = Constants.ArticleAuthorizationFailureMessage;
                    return response;
                }

                article.Title = request.Title;
                article.Content = request.Content;

                var updatedArticle = await _repository.UpdateAsync(article);
                try
                {
                    var db = _redis.GetDatabase();
                    await db.KeyDeleteAsync($"article:{articleId}");
                }
                catch (RedisException ex)
                {
                    _logger.LogWarning(ex, "Redis server unavailable for cache invalidation in UpdateArticleAsync, articleId: {ArticleId}", articleId);
                }

                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.ArticleUpdateSuccessMessage;
                response.Data = MapToArticleResponse(updatedArticle);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article {ArticleId}", articleId);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = Constants.DefaultExceptionFriendlyMessage;
                return response;
            }
        }

        public async Task<Response<bool>> DeleteArticleAsync(string articleId, string userId)
        {
            var response = new Response<bool>();
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = Constants.UserNotFoundMessage;
                    return response;
                }

                var article = await _repository.GetByIdAsync(articleId);
                if (article is null)
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = Constants.ArticleNotFoundMessage;
                    return response;
                }

                if (article.AuthorId != userId)
                {
                    response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Message = Constants.ArticleAuthorizationFailureMessage;
                    return response;
                }

                await _repository.DeleteAsync(articleId);
                try
                {
                    var db = _redis.GetDatabase();
                    await db.KeyDeleteAsync($"article:{articleId}");
                }
                catch (RedisException ex)
                {
                    _logger.LogWarning(ex, "Redis server unavailable for cache invalidation in UpdateArticleAsync, articleId: {ArticleId}", articleId);
                }

                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.ArticleDeletionSuccessMessage;
                response.Data = true;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article {ArticleId}", articleId);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = Constants.DefaultExceptionFriendlyMessage;
                return response;
            }
        }

        private static ArticleResponse MapToArticleResponse(Article article)
        {
            return new ArticleResponse
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                AuthorId = article.AuthorId,
                AuthorName = article.Author?.FirstName + " " + article.Author?.LastName,
                LikesCount = article.Likes?.Count ?? 0,
                CreatedAt = article.CreatedAt,
                ModifiedAt = article.ModifiedAt
            };
        }
    }
}
