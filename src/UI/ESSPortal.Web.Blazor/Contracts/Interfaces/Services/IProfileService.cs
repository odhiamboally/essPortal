using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Profile;
using ESSPortal.Web.Blazor.Dtos.Profile;
using ESSPortal.Web.Blazor.ViewModels.Profile;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface IProfileService
{
    Task<ApiResponse<UserProfileResponse>> GetUserProfileAsync(string userId);
    Task<ApiResponse<bool>> UpdatePersonalDetailsAsync(UpdatePersonalDetailsRequest request);
    Task<ApiResponse<bool>> UpdateContactInfoAsync(UpdateContactInfoRequest request);
    Task<ApiResponse<bool>> UpdateBankingInfoAsync(UpdateBankingInfoRequest request);
    Task<ApiResponse<string>> UpdateProfilePictureAsync(UpdateProfilePictureViewModel viewModel);
    
}
