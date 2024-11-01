using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorebaseLikeFeature.Application.DTOs.Auth;
using NorebaseLikeFeature.Application.DTOs.Response;
using NorebaseLikeFeature.Application.Interfaces.IServices;
using NorebaseLikeFeature.Common.Config;
using NorebaseLikeFeature.Common.Responses;
using NorebaseLikeFeature.Common.Utilities;
using NorebaseLikeFeature.Domain.User;

namespace NorebaseLikeFeature.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<AuthSettings> _jwtData;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<AuthSettings> jwtData,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _jwtData = jwtData;
            _logger = logger;
        }

        public async Task<Response<RegisterResponse>> RegisterAsync(RegisterRequest request)
        {
            var response = new Response<RegisterResponse>();
            try
            {
                var normalizedEmail = request.Email.ToLowerInvariant();
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser is not null)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = Constants.UserAlreadyExistsMessage;
                    return response;
                }

                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError("Error creating user: {ErrorCode}, {ErrorDescription}",
                            error.Code, error.Description);
                    }
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Message = Constants.FailedMessage;
                    return response;
                }
                var registerResponse = new RegisterResponse
                {
                    Id = user.Id,
                    Email = user.Email
                };

                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.SuccessMessage;
                response.Data = registerResponse;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user {Email}", request.Email);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = Constants.DefaultExceptionFriendlyMessage;
                return response;
            }
        }

        public async Task<Response<AuthResponse>> LoginAsync(LoginRequest request)
        {
            var response = new Response<AuthResponse>();
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = Constants.InvalidCredentialsMessage;
                    return response;
                }

                var authResponse = new AuthResponse
                {
                    Token = TokenService.GenerateJwtToken(user, _jwtData),
                    Username = user.UserName,
                    Email = user.Email
                };

                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.SuccessMessage;
                response.Data = authResponse;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in user {Email}", request.Email);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = Constants.DefaultExceptionFriendlyMessage;
                return response;
            }
        }
    }
}

