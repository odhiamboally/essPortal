using ESSPortal.Infrastructure.Configuration;

using FluentValidation;

namespace ESSPortal.Infrastructure.Validations;


public class EmailSettingsValidator : AbstractValidator<EmailSettings>
{
    public EmailSettingsValidator()
    {
        RuleFor(x => x.SmtpServer)
            .NotEmpty().WithMessage("SMTP Server is required")
            .MaximumLength(255).WithMessage("SMTP Server cannot exceed 255 characters");
            

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535")
            .Must(BeCommonSmtpPort).WithMessage("Port should be a common SMTP port (25, 465, or 587) for better compatibility")
            .WithSeverity(Severity.Warning); // This is a warning, not an error

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(320).WithMessage("Username cannot exceed 320 characters")
            .EmailAddress().WithMessage("Username should typically be a valid email address for most SMTP providers")
            .WithSeverity(Severity.Warning); // Warning since not all SMTP servers require email format

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(3).WithMessage("Password must be at least 3 characters")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters");
            

        RuleFor(x => x.FromAddress)
            .NotEmpty().WithMessage("From Address is required")
            .EmailAddress().WithMessage("From Address must be a valid email address");
            

        RuleFor(x => x.ClientBaseUrl)
            .NotEmpty().WithMessage("Client Base URL is required")
            .Must(BeValidHttpUrl).WithMessage("Client Base URL must be a valid HTTP or HTTPS URL");
            

        RuleFor(x => x.DisplayName)
            .MaximumLength(100).WithMessage("Display Name cannot exceed 100 characters")
            .Must(NotContainInvalidDisplayNameCharacters).WithMessage("Display Name contains invalid characters (control characters, <, >, or \")")
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleFor(x => x.EmailTemplatePath)
            .MaximumLength(500).WithMessage("Email Template Path cannot exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.EmailTemplatePath));

        // Business rule: Domain consistency between FromAddress and Username
        RuleFor(x => x)
            .Must(HaveConsistentDomains).WithMessage("From Address and Username should typically use the same domain for better SMTP compatibility")
            .WithSeverity(Severity.Warning)
            .When(x => IsValidEmail(x.FromAddress) && IsValidEmail(x.Username));
    }


    private static bool BeValidHttpUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    private static bool BeCommonSmtpPort(int port)
    {
        return port is 25 or 465 or 587;
    }

    private static bool NotContainInvalidDisplayNameCharacters(string displayName)
    {
        return !displayName.Any(c => char.IsControl(c) || c == '<' || c == '>' || c == '"');
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var mailAddress = new System.Net.Mail.MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool HaveConsistentDomains(EmailSettings settings)
    {
        if (!IsValidEmail(settings.FromAddress) || !IsValidEmail(settings.Username))
            return true; // Skip validation if either is not a valid email

        var fromDomain = settings.FromAddress.Split('@')[1];
        var usernameDomain = settings.Username.Split('@')[1];

        return fromDomain.Equals(usernameDomain, StringComparison.OrdinalIgnoreCase);
    }
}
