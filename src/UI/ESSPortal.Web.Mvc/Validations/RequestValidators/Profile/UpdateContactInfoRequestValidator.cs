using ESSPortal.Web.Mvc.Dtos.Profile;

using FluentValidation;

using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ESSPortal.Web.Mvc.Validations.RequestValidators.Profile;

public class UpdateContactInfoRequestValidator : AbstractValidator<UpdateContactInfoRequest>
{
    public UpdateContactInfoRequestValidator()
    {
        RuleFor(x => x)
            .Must(HaveAtLeastOnePhoneNumber)
            .WithMessage("At least one phone number is required.")
            .WithName("PhoneNumbers");

        RuleFor(x => x.MobileNo)
            .Must(BeValidPhoneNumber)
            .WithMessage("Invalid mobile number format.")
            .When(x => !string.IsNullOrWhiteSpace(x.MobileNo));

        RuleFor(x => x.TelephoneNo)
            .Must(BeValidPhoneNumber)
            .WithMessage("Invalid telephone number format.")
            .When(x => !string.IsNullOrWhiteSpace(x.TelephoneNo));

        RuleFor(x => x.ContactEMailAddress)
            .Must(BeValidEmail)
            .WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEMailAddress));

        RuleFor(x => x.PhysicalAddress)
            .MaximumLength(200)
            .WithMessage("Physical address cannot exceed 200 characters.");

        RuleFor(x => x.PostalAddress)
            .MaximumLength(200)
            .WithMessage("Postal address cannot exceed 200 characters.");

        RuleFor(x => x.City)
            .MaximumLength(50)
            .WithMessage("City cannot exceed 50 characters.");

        RuleFor(x => x.PostCode)
            .MaximumLength(20)
            .WithMessage("Post code cannot exceed 20 characters.");

        RuleFor(x => x.CountryRegionCode)
            .MaximumLength(10)
            .WithMessage("Country region code cannot exceed 10 characters.");
    }

    private bool HaveAtLeastOnePhoneNumber(UpdateContactInfoRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.MobileNo) ||
               !string.IsNullOrWhiteSpace(request.TelephoneNo);
    }

    private bool BeValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return true;

        // Remove common formatting characters
        var cleanNumber = Regex.Replace(phoneNumber, @"[\s\-\(\)\+\.]", "");

        // Check if it contains only digits and is of reasonable length
        return Regex.IsMatch(cleanNumber, @"^\d{7,15}$");
    }

    private bool BeValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return true;

        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
