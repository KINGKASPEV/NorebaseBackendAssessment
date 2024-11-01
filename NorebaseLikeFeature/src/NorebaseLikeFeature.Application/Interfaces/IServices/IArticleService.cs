using NorebaseLikeFeature.Application.DTOs.Article;
using NorebaseLikeFeature.Application.DTOs.Response;
using NorebaseLikeFeature.Common.Responses;

namespace NorebaseLikeFeature.Application.Interfaces.IServices
{
    public interface IArticleService
    {
        Task<Response<ArticleResponse>> GetArticleAsync(string articleId);
        Task<Response<List<ArticleResponse>>> GetAllArticlesAsync();
        Task<Response<ArticleResponse>> CreateArticleAsync(CreateArticleRequest request, string userId);
        Task<Response<ArticleResponse>> UpdateArticleAsync(string articleId, UpdateArticleRequest request, string userId);
        Task<Response<bool>> DeleteArticleAsync(string articleId, string userId);
    }
}
