using EssPortal.Web.Mvc.Dtos.Auth;

using FluentValidation;

namespace ESSPortal.Web.Mvc.Validations.RequestValidators.Auth;

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
