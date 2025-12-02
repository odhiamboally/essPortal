namespace ESSPortal.Application.Dtos.Profile;
public record UpdateProfilePictureRequest(
    string UserId,
    string FileName,
    string ContentType,
    string Base64Content
);
