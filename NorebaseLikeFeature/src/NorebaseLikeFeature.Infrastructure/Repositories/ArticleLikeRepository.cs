using Microsoft.EntityFrameworkCore;
using NorebaseLikeFeature.Application.Interfaces.IRepositories;
using NorebaseLikeFeature.Domain.Article;
using NorebaseLikeFeature.Infrastructure.Context;

namespace NorebaseLikeFeature.Infrastructure.Repositories
{
    public class ArticleLikeRepository : IArticleLikeRepository
    {
        private readonly AppDbContext _context;

        public ArticleLikeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ArticleLike> GetLikeAsync(string articleId, string userId)
        {
            return await _context.ArticleLikes
                .FirstOrDefaultAsync(l => l.ArticleId == articleId && l.UserId == userId);
        }

        public async Task<int> GetLikeCountAsync(string articleId)
        {
            return await _context.ArticleLikes
                .Where(l => l.ArticleId == articleId)
                .CountAsync();
        }

        public async Task<ArticleLike> AddLikeAsync(ArticleLike like)
        {
            await _context.ArticleLikes.AddAsync(like);
            await _context.SaveChangesAsync();
            return like;
        }

        public async Task RemoveLikeAsync(ArticleLike like)
        {
            _context.ArticleLikes.Remove(like);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasUserLikedAsync(string articleId, string userId)
        {
            return await _context.ArticleLikes
                .AnyAsync(l => l.ArticleId == articleId && l.UserId == userId);
        }
    }
}
