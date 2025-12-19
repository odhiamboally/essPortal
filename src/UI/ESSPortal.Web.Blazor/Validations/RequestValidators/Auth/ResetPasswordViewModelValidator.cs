using EssPortal.Web.Blazor.Dtos.Auth;

using ESSPortal.Web.Blazor.ViewModels.Auth;

using FluentValidation;

namespace ESSPortal.Web.Blazor.Validations.RequestValidators.Auth;

public class ResetPasswordViewModelValidator : AbstractValidator<ResetPasswordViewModel>
{
    public ResetPasswordViewModelValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;



        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.NewPassword)
             .NotEmpty().WithMessage("Password is required.")

             .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
             .MaximumLength(100).WithMessage("The password cannot exceed 100 characters.")
             .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
             .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
            
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
            


    }
}

