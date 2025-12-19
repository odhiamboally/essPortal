using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Blazor.ViewModels.Auth;

public record ForgotPasswordViewModel(
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [RegularExpression(@"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$",
        ErrorMessage = "Please enter a valid email address")]
    string Email,

    string? LogoBase64

    );
