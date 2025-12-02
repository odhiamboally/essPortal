using ESSPortal.Infrastructure.Validations;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ESSPortal.Infrastructure.Extensions;


/// <summary>
/// Extension method to integrate FluentValidation with IOptions
/// </summary>
public static class FluentValidationOptionsExtensions
{
    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(this OptionsBuilder<TOptions> optionsBuilder)
        where TOptions : class
    {
        optionsBuilder.Services.TryAddSingleton<IValidateOptions<TOptions>>(provider =>
        {
            using var scope = provider.CreateScope(); // Safe scope creation
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();
            return new FluentValidationOptionsValidator<TOptions>(validator);
        });

        return optionsBuilder;
    }
}

