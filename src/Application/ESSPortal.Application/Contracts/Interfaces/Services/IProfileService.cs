using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Profile;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IProfileService
{
    // Read operations
    Task<ApiResponse<UserProfileResponse>> GetUserProfileAsync(string userId);
    Task<ApiResponse<bool>> ValidateProfileDataAsync(string userId);
    Task<ApiResponse<int>> CalculateProfileCompletionAsync(string userId);

    // Update operations
    Task<ApiResponse<bool>> UpdatePersonalDetailsAsync(UpdatePersonalDetailsRequest request);
    Task<ApiResponse<bool>> UpdateContactInfoAsync(UpdateContactInfoRequest request);
    Task<ApiResponse<bool>> UpdateBankingInfoAsync(UpdateBankingInfoRequest request);

    // Create operations
    Task<ApiResponse<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request);
}
