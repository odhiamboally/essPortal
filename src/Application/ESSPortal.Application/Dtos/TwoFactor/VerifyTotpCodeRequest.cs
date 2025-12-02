namespace ESSPortal.Application.Dtos.TwoFactor;
public record VerifyTotpCodeRequest
{
    public string UserId { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}

