using Microsoft.AspNetCore.Mvc;
using NorebaseLikeFeature.Application.Interfaces.IServices;
using System.Security.Claims;

namespace NorebaseLikeFeature.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpGet("{articleId}")]
        public async Task<IActionResult> GetLikes(string articleId)
        {
            var userId = User.Identity?.IsAuthenticated == true ?
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
            var response = await _likeService.GetLikesAsync(articleId, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{articleId}/toggle")]
        public async Task<IActionResult> ToggleLike(string articleId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await _likeService.ToggleLikeAsync(articleId, userId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
