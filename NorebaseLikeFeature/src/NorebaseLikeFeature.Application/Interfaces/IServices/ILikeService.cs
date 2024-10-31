using NorebaseLikeFeature.Application.DTOs.Response;
using NorebaseLikeFeature.Common.Responses;

namespace NorebaseLikeFeature.Application.Interfaces.IServices
{
    public interface ILikeService
    {
        Task<Response<LikeResponse>> GetLikesAsync(string articleId, string userId);
        Task<Response<LikeResponse>> ToggleLikeAsync(string articleId, string userId);
    }
}
