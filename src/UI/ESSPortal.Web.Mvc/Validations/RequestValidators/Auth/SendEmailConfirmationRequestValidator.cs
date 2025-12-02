using EssPortal.Web.Mvc.Dtos.Auth;

using FluentValidation;

namespace ESSPortal.Web.Mvc.Validations.RequestValidators.Auth;

public class SendEmailConfirmationRequestValidator : AbstractValidator<SendEmailConfirmationRequest>
{
    public SendEmailConfirmationRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("UserId is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(254).WithMessage("Email address cannot exceed 254 characters."); 
                






    }
}
