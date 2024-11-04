using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorebaseLikeFeature.Application.DTOs.Article;
using NorebaseLikeFeature.Application.Interfaces.IServices;

namespace NorebaseLikeFeature.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticlesController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet("{articleId}")]
        public async Task<IActionResult> GetArticle(string articleId)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);

            var response = await _articleService.GetArticleAsync(articleId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllArticles()
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);

            var response = await _articleService.GetAllArticlesAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateArticle(string userId, [FromBody] CreateArticleRequest request)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);

            var response = await _articleService.CreateArticleAsync(request, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{articleId}")]
        public async Task<IActionResult> UpdateArticle(string articleId, string userId, [FromBody] UpdateArticleRequest request)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);

            var response = await _articleService.UpdateArticleAsync(articleId, request, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{articleId}")]
        public async Task<IActionResult> DeleteArticle(string articleId, string userId)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);

            var response = await _articleService.DeleteArticleAsync(articleId, userId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
