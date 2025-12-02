using FluentValidation;

using Microsoft.Extensions.Options;

namespace ESSPortal.Infrastructure.Validations;

/// <summary>
/// Bridge between FluentValidation and IValidateOptions
/// </summary>
public class FluentValidationOptionsValidator<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly IValidator<TOptions>? _validator;

    public FluentValidationOptionsValidator(IValidator<TOptions>? validator)
    {
        _validator = validator;
    }

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        if (_validator == null)
            return ValidateOptionsResult.Success;

        var validationResult = _validator.Validate(options);

        if (validationResult.IsValid)
            return ValidateOptionsResult.Success;

        var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
        return ValidateOptionsResult.Fail(errors);
    }
}
