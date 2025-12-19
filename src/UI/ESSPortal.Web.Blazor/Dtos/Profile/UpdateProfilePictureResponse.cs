namespace ESSPortal.Web.Blazor.Dtos.Profile;

public record UpdateProfilePictureResponse(
    string UserId,
    string FileName,
    string ContentType,
    string Base64Content
);
