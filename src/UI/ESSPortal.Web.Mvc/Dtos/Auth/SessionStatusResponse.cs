namespace ESSPortal.Web.Mvc.Dtos.Auth;

public record SessionStatusResponse
{
    public bool IsValid { get; init; }
    public string? UserId { get; init; }
    public string? SessionId { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}
