namespace ESSPortal.Application.Dtos.TwoFactor;
public record EnableTwoFactorRequest
{
    public string VerificationCode { get; init; } = string.Empty;
}
