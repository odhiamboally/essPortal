using EssPortal.Web.Blazor.Dtos.Auth;

using ESSPortal.Application.Dtos.Auth;

using FluentValidation;

namespace ESSPortal.Web.Blazor.Validations.RequestValidators.Auth;

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmUserEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("UserId is required.");


    }
}

