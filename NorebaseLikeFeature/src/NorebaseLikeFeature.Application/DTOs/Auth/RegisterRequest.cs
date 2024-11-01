using System.ComponentModel.DataAnnotations;

namespace NorebaseLikeFeature.Application.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; } = String.Empty;

        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; } = String.Empty;
    }
}
