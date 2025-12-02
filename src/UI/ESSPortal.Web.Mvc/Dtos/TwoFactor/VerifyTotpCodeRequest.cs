using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Mvc.Dtos.TwoFactor;

public record VerifyTotpCodeRequest
{
    public string UserId { get; init; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be exactly 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must contain only digits")]
    public string Code { get; init; } = string.Empty;
}
