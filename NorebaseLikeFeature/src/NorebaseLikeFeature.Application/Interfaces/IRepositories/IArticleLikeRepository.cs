using NorebaseLikeFeature.Domain.Article;

namespace NorebaseLikeFeature.Application.Interfaces.IRepositories
{
    public interface IArticleLikeRepository
    {
        Task<ArticleLike> GetLikeAsync(string articleId, string userId);
        Task<int> GetLikeCountAsync(string articleId);
        Task<ArticleLike> AddLikeAsync(ArticleLike like);
        Task RemoveLikeAsync(ArticleLike like);
        Task<bool> HasUserLikedAsync(string articleId, string userId);
    }
}
