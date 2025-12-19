namespace ESSPortal.Web.Blazor.ViewModels.Profile;

public class UpdateProfilePictureViewModel
{
    public string UserId { get; set; } = string.Empty;
    public IFormFile? ProfilePicture { get; set; } // For MVC controller
    public string FileName { get; set; } = string.Empty; // For API calls
    public string ContentType { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty; // For API calls
}
