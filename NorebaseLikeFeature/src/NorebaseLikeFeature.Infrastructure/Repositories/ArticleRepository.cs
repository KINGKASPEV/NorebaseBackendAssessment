using Microsoft.EntityFrameworkCore;
using NorebaseLikeFeature.Application.Interfaces.IRepositories;
using NorebaseLikeFeature.Domain.Article;
using NorebaseLikeFeature.Persistence.Context;

namespace NorebaseLikeFeature.Persistence.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly AppDbContext _context;

        public ArticleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Article> GetByIdAsync(string id)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Likes)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Article>> GetAllAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Likes)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Article> CreateAsync(Article article)
        {
            await _context.Articles.AddAsync(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Article> UpdateAsync(Article article)
        {
            article.ModifiedAt = DateTime.UtcNow;
            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task DeleteAsync(string id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Articles.AnyAsync(a => a.Id == id);
        }
    }
}

