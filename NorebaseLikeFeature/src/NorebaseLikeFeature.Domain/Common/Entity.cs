namespace NorebaseLikeFeature.Domain.Common
{
    public class Entity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
        public DateTime? ModifiedAt { get; set; }
    }
}
