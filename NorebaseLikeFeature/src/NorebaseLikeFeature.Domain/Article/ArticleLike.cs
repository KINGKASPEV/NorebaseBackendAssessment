using NorebaseLikeFeature.Domain.Common;

namespace NorebaseLikeFeature.Domain.Article
{
    public class ArticleLike : Entity
    {
        public string ArticleId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
