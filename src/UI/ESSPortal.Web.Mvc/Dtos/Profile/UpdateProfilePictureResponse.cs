namespace ESSPortal.Web.Mvc.Dtos.Profile;

public record UpdateProfilePictureResponse(
    string UserId,
    string FileName,
    string ContentType,
    string Base64Content
);
