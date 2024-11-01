using NorebaseLikeFeature.Application.DTOs.Auth;
using NorebaseLikeFeature.Application.DTOs.Response;
using NorebaseLikeFeature.Common.Responses;

namespace NorebaseLikeFeature.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<Response<RegisterResponse>> RegisterAsync(RegisterRequest request);
        Task<Response<AuthResponse>> LoginAsync(LoginRequest request);
    }
}
