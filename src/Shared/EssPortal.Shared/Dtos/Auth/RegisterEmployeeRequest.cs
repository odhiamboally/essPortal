using Microsoft.AspNetCore.Authentication;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EssPortal.Shared.Dtos.Auth;

public record RegisterEmployeeRequest
{
    [Required(ErrorMessage = "Employee Number is required")]
    [Display(Name = "Employee Number")]
    public string EmployeeNumber { get; init; } = string.Empty;

    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Gender { get; init; }

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

    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public string? ReturnUrl { get; init; }
    public List<AuthenticationScheme> ExternalLogins { get; init; } = [];
}

