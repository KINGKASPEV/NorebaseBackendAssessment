using NorebaseLikeFeature.Application.DTOs;
using NorebaseLikeFeature.Application.DTOs.Response;
using NorebaseLikeFeature.Common.Responses;

namespace NorebaseLikeFeature.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<Response<AuthResponse>> RegisterAsync(RegisterRequest request);
        Task<Response<AuthResponse>> LoginAsync(LoginRequest request);
    }
}
