namespace ESSPortal.Web.Blazor.ViewModels.TwoFactor;

public class TwoFactorSettingsViewModel
{
    public bool IsTwoFactorEnabled { get; set; }
    public bool HasBackupCodes { get; set; }
    public bool CanEnable2FA { get; set; }
}
