using Microsoft.AspNetCore.Identity;

namespace NorebaseLikeFeature.Domain.User
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public DateTime DateCreated { get; set; } = DateTime.Now.ToUniversalTime();
        public DateTime? DateModified { get; set; }
    }
}
