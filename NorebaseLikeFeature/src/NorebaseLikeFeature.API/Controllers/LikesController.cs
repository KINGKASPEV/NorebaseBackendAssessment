using Microsoft.AspNetCore.Mvc;
using NorebaseLikeFeature.Application.Interfaces.IServices;

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
        public async Task<IActionResult> GetLikes(string articleId, string userId)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);

            var response = await _likeService.GetLikesAsync(articleId, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{articleId}/toggle")]
        public async Task<IActionResult> ToggleLike(string articleId, string userId)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);

            var response = await _likeService.ToggleLikeAsync(articleId, userId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
