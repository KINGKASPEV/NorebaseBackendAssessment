using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorebaseLikeFeature.Application.DTOs.Auth;
using NorebaseLikeFeature.Application.Interfaces.IServices;

namespace NorebaseLikeFeature.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);

            var response = await _authService.RegisterAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);

            var response = await _authService.LoginAsync(request);
            return StatusCode(response.StatusCode, response);
        }
    }
}
