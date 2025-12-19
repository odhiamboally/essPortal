using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Web.Blazor.Dtos.Profile;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface IFileService
{
    Task<(string base64String, string imageDataUrl, byte[] imageBytes)> ReadLogoAsync();
    Task<AppResponse<ProfilePictureResponse>> SaveProfilePictureAsync(string userId, byte[] imageBytes, string originalFileName);
    Task<(byte[] imageBytes, string contentType)> ReadProfilePictureAsync(string fileName);
    Task<(string base64String, string imageDataUrl)> ReadProfilePictureAsBase64Async(string fileName);
    bool DeleteProfilePicture(string fileName);
    string ExtractFileNameFromUrl(string profilePictureUrl);
}
