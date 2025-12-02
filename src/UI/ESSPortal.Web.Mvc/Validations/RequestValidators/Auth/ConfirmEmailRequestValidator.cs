using EssPortal.Web.Mvc.Dtos.Auth;

using FluentValidation;

namespace ESSPortal.Web.Mvc.Validations.RequestValidators.Auth;

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

