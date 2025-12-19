using EssPortal.Web.Blazor.Dtos.Auth;

using ESSPortal.Web.Blazor.ViewModels.Auth;

using FluentValidation;

namespace ESSPortal.Web.Blazor.Validations.RequestValidators.Auth;

public class ForgotPasswordViewModelValidator : AbstractValidator<ForgotPasswordViewModel>
{
    public ForgotPasswordViewModelValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("Email is required.") 
            .EmailAddress().WithMessage("Invalid email format.") 
            .Matches(@"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$")
            .WithMessage("Not a valid Email Address (does not match required pattern).");
                


    }
}

