using EssPortal.Web.Mvc.Dtos.Auth;

using FluentValidation;

namespace ESSPortal.Web.Mvc.Validations.RequestValidators.Auth;

public class Send2FACodeRequestValidator : AbstractValidator<Send2FACodeRequest>
{
    public Send2FACodeRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Provider is required.");
            
        RuleFor(x => x.SelectedProvider)
            .NotEmpty().WithMessage("Please select a verification method");
            


    }
}

