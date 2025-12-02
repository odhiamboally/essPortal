using System.ComponentModel.DataAnnotations;

namespace EssPortal.Web.Mvc.Dtos.Auth;

public class ResetPasswordRequest
{
    [Required]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
    [Display(Name = "New Password")]
    public string? NewPassword { get; init; }

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match")]
    [Display(Name = "Confirm Password")]
    public string? ConfirmPassword { get; init; }

    [Required]
    public string Token { get; init; } = string.Empty;

    // Include logo data from frontend
    public string? LogoBase64  { get; set; }
}
