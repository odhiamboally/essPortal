using EssPortal.Web.Mvc.Dtos.Auth;

using FluentValidation;

namespace ESSPortal.Web.Mvc.Validations.RequestValidators.Auth;

public class Verify2FACodeRequestValidator : AbstractValidator<Verify2FACodeRequest>
{
    public Verify2FACodeRequestValidator()
    {

        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        // Make Provider optional for TOTP flows
        When(x => !IsTotpProvider(x.ProviderDisplayName), () =>
        {
            RuleFor(x => x.Provider)
                .NotEmpty().WithMessage("Provider is required");
        });

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(6, 8).WithMessage("Verification code must be 6-8 characters long")
            .Matches("^[0-9]+$").WithMessage("Verification code can only contain numbers")
            .Must(BeValidCodeFormat).WithMessage("Invalid verification code format");
            

    }

    private static bool BeValidCodeFormat(string code)
    {
        // Support different code formats
        return code.Length >= 4 && code.Length <= 8 && code.All(char.IsDigit);
    }

    private bool IsTotpProvider(string providerDisplayName)
    {
        return providerDisplayName?.Contains("Authenticator", StringComparison.OrdinalIgnoreCase) == true;
    }
}
