using NorebaseLikeFeature.Domain.Common;
using NorebaseLikeFeature.Domain.User;

namespace NorebaseLikeFeature.Domain.Article
{
    public class ArticleLike : Entity
    {
        public string ArticleId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public Article Article { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
