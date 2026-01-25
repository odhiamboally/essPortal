namespace ESSPortal.Shared.Dtos.TwoFactor;

public record EnableTwoFactorRequest
{
    public string VerificationCode { get; init; } = string.Empty;
}