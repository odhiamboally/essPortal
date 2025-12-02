using ESSPortal.Web.Mvc.Dtos.Profile;

using FluentValidation;

namespace ESSPortal.Web.Mvc.Validations.RequestValidators.Profile;

public class UpdateBankingInfoRequestValidator : AbstractValidator<UpdateBankingInfoRequest>
{
    public UpdateBankingInfoRequestValidator()
    {
        // If bank account number is provided, other fields are required
        RuleFor(x => x.KBABankCode)
            .NotEmpty()
            .WithMessage("Bank code is required when account number is provided.")
            .Length(6)
            .WithMessage("Bank code must be exactly 6 digits.")
            .Matches(@"^\d+$")
            .WithMessage("Bank code must contain only digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.BankAccountNo));

        RuleFor(x => x.KBABranchCode)
            .NotEmpty()
            .WithMessage("Branch code is required when account number is provided.")
            .Length(3)
            .WithMessage("Branch code must be exactly 3 digits.")
            .Matches(@"^\d+$")
            .WithMessage("Branch code must contain only digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.BankAccountNo));

        RuleFor(x => x.BankAccountType)
            .NotEmpty()
            .WithMessage("Account type is required when account number is provided.")
            .Must(BeValidAccountType)
            .WithMessage("Please select a valid account type.")
            .When(x => !string.IsNullOrWhiteSpace(x.BankAccountNo));

        RuleFor(x => x.BankAccountNo)
            .Length(8, 17)
            .WithMessage("Bank account number must be between 8 and 17 digits.")
            .Matches(@"^\d+$")
            .WithMessage("Bank account number must contain only digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.BankAccountNo));

        // Validate individual fields when provided
        RuleFor(x => x.KBABankCode)
            .Length(6)
            .WithMessage("Bank code must be exactly 6 digits.")
            .Matches(@"^\d+$")
            .WithMessage("Bank code must contain only digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.KBABankCode));

        RuleFor(x => x.KBABranchCode)
            .Length(3)
            .WithMessage("Branch code must be exactly 3 digits.")
            .Matches(@"^\d+$")
            .WithMessage("Branch code must contain only digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.KBABranchCode));

        RuleFor(x => x.BankAccountType)
            .Must(BeValidAccountType)
            .WithMessage("Please select a valid account type.")
            .When(x => !string.IsNullOrWhiteSpace(x.BankAccountType));
    }

    private bool BeValidAccountType(string? accountType)
    {
        if (string.IsNullOrWhiteSpace(accountType)) return true;

        string[] validAccountTypes = ["Savings", "Current", "Checking", "Fixed Deposit"];
        return validAccountTypes.Contains(accountType, StringComparer.OrdinalIgnoreCase);
    }
}
