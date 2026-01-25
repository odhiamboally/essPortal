using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Profile;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface IProfileService
{
    Task<AppResponse<UserProfileResponse>> GetUserProfileAsync(string userId);
    Task<AppResponse<bool>> UpdatePersonalDetailsAsync(UpdatePersonalDetailsRequest request);
    Task<AppResponse<bool>> UpdateContactInfoAsync(UpdateContactInfoRequest request);
    Task<AppResponse<bool>> UpdateBankingInfoAsync(UpdateBankingInfoRequest request);
    Task<AppResponse<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request);
    
}
