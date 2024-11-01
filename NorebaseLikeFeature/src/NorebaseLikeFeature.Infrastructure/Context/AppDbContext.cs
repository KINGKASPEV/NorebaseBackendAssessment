using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NorebaseLikeFeature.Domain.Article;
using NorebaseLikeFeature.Domain.User;

namespace NorebaseLikeFeature.Persistence.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleLike> ArticleLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Article>(entity =>
            {
                entity.HasOne(a => a.Author)
                    .WithMany()
                    .HasForeignKey(a => a.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ArticleLike>(entity =>
            {
                entity.HasIndex(a => new { a.ArticleId, a.UserId })
                    .IsUnique();

                entity.HasOne(al => al.Article)
                    .WithMany(a => a.Likes)
                    .HasForeignKey(al => al.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(al => al.User)
                    .WithMany()
                    .HasForeignKey(al => al.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
