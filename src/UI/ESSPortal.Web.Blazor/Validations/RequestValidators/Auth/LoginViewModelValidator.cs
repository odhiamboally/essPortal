using EssPortal.Web.Blazor.Dtos.Auth;

using ESSPortal.Web.Blazor.ViewModels.Auth;

using FluentValidation;

namespace ESSPortal.Web.Blazor.Validations.RequestValidators.Auth;

public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
{
    public LoginViewModelValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.EmployeeNumber)
            .NotNull().WithMessage("Employee Number is required.")
            .NotEmpty().WithMessage("Employee Number is required.")
            .MinimumLength(1).WithMessage("Employee Number must be at least 1 characters long.")
            .Matches("^[a-zA-Z0-9]+$").WithMessage("Employee Number can only contain letters and numbers")
            .MaximumLength(10).WithMessage("Employee Number must not exceed 10 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 6 characters long.")
            .MaximumLength(20).WithMessage("The password cannot exceed 20 characters.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d])[A-Za-z\d\S]{8,}$")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number.");


    }
}