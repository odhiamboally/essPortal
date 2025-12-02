namespace ESSPortal.Web.Mvc.Dtos.TwoFactor;

public record EnableTwoFactorRequest
{
    public string VerificationCode { get; init; } = string.Empty;
}