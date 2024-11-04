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
using NorebaseLikeFeature.Domain.Enums;
using NorebaseLikeFeature.Domain.User;

namespace NorebaseLikeFeature.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<AuthSettings> _authSettings;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<AuthSettings> authSettings,
            RoleManager<IdentityRole> roleManager,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _authSettings = authSettings;
            _roleManager = roleManager;
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
                    LastName = request.LastName,
                    UserRole = UserRole.User
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError("Error creating user: {ErrorCode}, {ErrorDescription}", error.Code, error.Description);
                    }
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Message = Constants.FailedMessage;
                    return response;
                }

                var roleExists = await _roleManager.RoleExistsAsync(UserRole.User.ToString());
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserRole.User.ToString()));
                }

                await _userManager.AddToRoleAsync(user, UserRole.User.ToString());

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

        public async Task<Response<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var response = new Response<LoginResponse>();
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user is null)
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = Constants.UserNotFoundMessage;
                    return response;
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!isPasswordValid)
                {
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = Constants.InvalidCredentialsMessage;
                    return response;
                }

                var roles = new string[] { user.UserRole.ToString() };
                var token = TokenService.GenerateToken(
                    user.Id,
                    user.Email,
                    roles,
                    _authSettings
                );

                var loginResponse = new LoginResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token
                };

                response.StatusCode = StatusCodes.Status200OK;
                response.Message = Constants.SuccessMessage;
                response.Data = loginResponse;

                _logger.LogInformation("User {Email} logged in successfully", request.Email);
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

