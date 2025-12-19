using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Blazor.ViewModels.TwoFactor;

public class TwoFactorSetupViewModel
{
    public string QrCodeUri { get; set; } = string.Empty;
    public string ManualEntryKey { get; set; } = string.Empty;
    public bool IsInitialSetup { get; set; } 

    [Required]
    [StringLength(6, MinimumLength = 6)]
    [Display(Name = "Verification Code")]
    public string VerificationCode { get; set; } = string.Empty;
}
