using NorebaseLikeFeature.Domain.Common;
using NorebaseLikeFeature.Domain.User;

namespace NorebaseLikeFeature.Domain.Article
{
    public class Article : Entity
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;

        public ApplicationUser Author { get; set; } = null!;
        public ICollection<ArticleLike> Likes { get; set; } = new List<ArticleLike>();
    }
}
