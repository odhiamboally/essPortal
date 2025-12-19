using ESSPortal.Application.Dtos.Profile;
using ESSPortal.Web.Blazor.Dtos.Profile;

using FluentValidation;

namespace ESSPortal.Web.Blazor.Validations.RequestValidators.Profile;

public class UpdatePersonalDetailsRequestValidator : AbstractValidator<UpdatePersonalDetailsRequest>
{
    public UpdatePersonalDetailsRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .MaximumLength(50)
            .WithMessage("First name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-'\.]+$")
            .WithMessage("First name contains invalid characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .MaximumLength(50)
            .WithMessage("Last name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-'\.]+$")
            .WithMessage("Last name contains invalid characters.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(50)
            .WithMessage("Middle name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-'\.]+$")
            .WithMessage("Middle name contains invalid characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.MiddleName));

        RuleFor(x => x.Gender)
            .Must(BeValidGender)
            .WithMessage("Please select a valid gender option.")
            .When(x => !string.IsNullOrWhiteSpace(x.Gender));
    }

    private bool BeValidGender(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender)) return true;

        string[] validGenders = ["Male", "Female", "Other", "Prefer not to say"];
        return validGenders.Contains(gender, StringComparer.OrdinalIgnoreCase);
    }
}

