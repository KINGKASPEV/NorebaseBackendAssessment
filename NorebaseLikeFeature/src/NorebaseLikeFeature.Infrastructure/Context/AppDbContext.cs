using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NorebaseLikeFeature.Domain.Article;
using NorebaseLikeFeature.Domain.User;

namespace NorebaseLikeFeature.Infrastructure.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<ArticleLike> ArticleLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ArticleLike>()
                .HasIndex(a => new { a.ArticleId, a.UserId })
                .IsUnique();
        }
    }
}
