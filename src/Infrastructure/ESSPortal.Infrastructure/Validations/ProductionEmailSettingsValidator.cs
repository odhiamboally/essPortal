using FluentValidation;

namespace ESSPortal.Infrastructure.Validations;
public class ProductionEmailSettingsValidator : EmailSettingsValidator
{
    public ProductionEmailSettingsValidator()
    {
        RuleFor(x => x.Port)
            .NotEqual(25)
            .WithMessage("Port 25 is not recommended for production use. Use port 587 or 465 for security.");

        RuleFor(x => x.ClientBaseUrl)
            .Must(BeHttpsUrl)
            .WithMessage("Client Base URL must use HTTPS in production environments")
            .When(x => !string.IsNullOrWhiteSpace(x.ClientBaseUrl));

        RuleFor(x => x.EnableSsl)
            .Equal(true)
            .WithMessage("SSL must be enabled in production environments");
    }

    private static bool BeHttpsUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               result.Scheme == Uri.UriSchemeHttps;
    }
}
