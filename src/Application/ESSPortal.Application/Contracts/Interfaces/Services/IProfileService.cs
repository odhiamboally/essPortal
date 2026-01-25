
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Profile;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IProfileService
{
    // Read operations
    Task<AppResponse<UserProfileResponse>> GetUserProfileAsync(string userId);
    Task<AppResponse<bool>> ValidateProfileDataAsync(string userId);
    Task<AppResponse<int>> CalculateProfileCompletionAsync(string userId);

    // Update operations
    Task<AppResponse<bool>> UpdatePersonalDetailsAsync(UpdatePersonalDetailsRequest request);
    Task<AppResponse<bool>> UpdateContactInfoAsync(UpdateContactInfoRequest request);
    Task<AppResponse<bool>> UpdateBankingInfoAsync(UpdateBankingInfoRequest request);

    // Create operations
    Task<AppResponse<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request);
}
