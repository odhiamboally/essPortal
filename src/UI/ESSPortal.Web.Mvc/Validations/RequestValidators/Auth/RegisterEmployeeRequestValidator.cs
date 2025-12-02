using EssPortal.Web.Mvc.Dtos.Auth;

using ESSPortal.Web.Mvc.Configurations;

using FluentValidation;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Validations.RequestValidators.Auth;

public class RegisterEmployeeRequestValidator : AbstractValidator<RegisterEmployeeRequest>
{
    private readonly EmailValidationSettings _emailSettings;
    public RegisterEmployeeRequestValidator(IOptions<EmailValidationSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;

        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        When(x => x != null, () => {

            RuleFor(x => x.EmployeeNumber)
                .NotEmpty()
                .WithMessage("Employee number is required.")
                .Matches("^SN[a-zA-Z0-9]+$")
                .WithMessage("Employee number must start with 'SN' and can only contain alphanumeric characters")
                .Length(4, 10).WithMessage("Employee number must be between 4 and 10 characters.");
                

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long.")
                .Length(8, 20).WithMessage("Password must be between 8 and 20 characters.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]+$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Password confirmation is required.")
                .Equal(x => x.Password)
                .WithMessage("Password and confirmation password do not match.");

            RuleFor(x => x.ReturnUrl)
                .Must(BeAValidLocalUrl).WithMessage("Return URL must be a valid local URL");


        });
    }

    private bool BeAValidLocalUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return url.StartsWith("~/") || url.StartsWith("/");
    }

    private static bool BeAValidBusinessEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string[] personalDomains =
        [
            /*"gmail.com",*/ "yahoo.com", "hotmail.com", "outlook.com",
                "aol.com", "icloud.com", "live.com", "msn.com", "ymail.com",
                "protonmail.com", "mail.com", "zoho.com"
        ];

        var domain = email.Split('@').LastOrDefault()?.ToLowerInvariant();
        return !personalDomains.Contains(domain);
    }

    private bool NotBeABlockedEmailDomain(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var domain = email.Split('@').LastOrDefault()?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        return !_emailSettings.BlockedDomains.Contains(domain);
    }
}


