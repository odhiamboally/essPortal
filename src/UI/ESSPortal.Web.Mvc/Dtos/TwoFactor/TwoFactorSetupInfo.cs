namespace ESSPortal.Web.Mvc.Dtos.TwoFactor;

public record TwoFactorSetupInfo
{
    public string QrCodeUri { get; init; } = string.Empty;
    public string ManualEntryKey { get; init; } = string.Empty;
}
