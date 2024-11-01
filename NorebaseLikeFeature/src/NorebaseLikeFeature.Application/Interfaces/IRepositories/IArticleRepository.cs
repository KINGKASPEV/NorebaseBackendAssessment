using NorebaseLikeFeature.Domain.Article;

namespace NorebaseLikeFeature.Application.Interfaces.IRepositories
{
    public interface IArticleRepository
    {
        Task<Article> GetByIdAsync(string id);
        Task<List<Article>> GetAllAsync();
        Task<Article> CreateAsync(Article article);
        Task<Article> UpdateAsync(Article article);
        Task DeleteAsync(string id);
    }
}
