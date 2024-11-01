using System.ComponentModel.DataAnnotations;

namespace NorebaseLikeFeature.Application.DTOs.Article
{
    public class UpdateArticleRequest
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(5000, ErrorMessage = "Content cannot exceed 5000 characters.")]
        public string Content { get; set; }
    }
}
