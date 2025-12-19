using EssPortal.Web.Blazor.Dtos.Auth;

using FluentValidation;

namespace ESSPortal.Web.Blazor.Validations.RequestValidators.Auth;

public class Get2FAProvidersRequestValidator : AbstractValidator<ProviderRequest>
{
    public Get2FAProvidersRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
