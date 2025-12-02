using Microsoft.AspNetCore.Authentication;

using System.ComponentModel.DataAnnotations;

namespace EssPortal.Web.Mvc.Dtos.Auth;

public record RegisterEmployeeRequest
{
    [Required(ErrorMessage = "Employee Number is required")]
    [Display(Name = "Employee Number")]
    public string EmployeeNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
    [Display(Name = "Password")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Confirm Password is required")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; init; } = string.Empty;

    public string? ReturnUrl { get; init; }
    public List<AuthenticationScheme> ExternalLogins { get; set; } = [];

    

}

