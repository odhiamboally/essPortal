using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Blazor.ViewModels.Auth;

public class LoginViewModel
{
    [Display(Name = "Employee Number")]
    [Required(ErrorMessage = "Employee Number is required.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "Employee Number must be between 1 and 10 characters.")]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Employee Number can only contain letters and numbers.")]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d])[A-Za-z\d\S]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }

    public string? DeviceFingerprint { get; set; }
}
