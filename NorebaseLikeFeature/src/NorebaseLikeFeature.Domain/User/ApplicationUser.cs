using Microsoft.AspNetCore.Identity;
using NorebaseLikeFeature.Domain.Enums;

namespace NorebaseLikeFeature.Domain.User
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public UserRole UserRole {  get; set; } 
        public DateTime DateCreated { get; set; } = DateTime.Now.ToUniversalTime();
        public DateTime? DateModified { get; set; }
    }
}
