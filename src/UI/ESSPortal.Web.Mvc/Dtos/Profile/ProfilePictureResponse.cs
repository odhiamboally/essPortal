namespace ESSPortal.Web.Mvc.Dtos.Profile;

public record ProfilePictureResponse
{
    public string UserId { get; init; } = string.Empty;
    public string PictureUrl { get; init; } = string.Empty;
    public string ImageDataUrl { get; init; } = string.Empty;
    public string Base64 { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}
