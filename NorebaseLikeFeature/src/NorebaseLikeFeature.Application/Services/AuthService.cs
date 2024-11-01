using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NorebaseLikeFeature.Application.DTOs.Auth;
using NorebaseLikeFeature.Application.DTOs.Response;
using NorebaseLikeFeature.Application.Interfaces.IServices;
using NorebaseLikeFeature.Common.Responses;
using NorebaseLikeFeature.Common.Utilities;
using NorebaseLikeFeature.Domain.User;

namespace NorebaseLikeFeature.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
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
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName
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
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateCreated = DateTime.Now.ToUniversalTime()
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
    }
}

