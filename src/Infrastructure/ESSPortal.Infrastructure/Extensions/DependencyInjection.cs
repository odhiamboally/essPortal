using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Infrastructure.Configuration;
using ESSPortal.Infrastructure.Implementations.Common;
using ESSPortal.Infrastructure.Implementations.Services;
using ESSPortal.Infrastructure.Validations;

using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Mail;

namespace ESSPortal.Infrastructure.Extensions;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            services.AddEmailSettings(configuration);
            services.AddFluentEmailWithSmtp(configuration);

            RegisterInfrastructureServices(services);

            return services;
        }
        catch (Exception)
        {
            throw;
        }

    }

    public static IServiceCollection AddEmailSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IValidator<EmailSettings>, EmailSettingsValidator>();

        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection("EmailSettings"))
            .ValidateFluentValidation()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddFluentEmailWithSmtp(this IServiceCollection services, IConfiguration configuration)
    {
        var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();

        if (emailSettings == null)
            throw new InvalidOperationException("EmailSettings section not found in configuration");

        services
            .AddFluentEmail(emailSettings.FromAddress, emailSettings.DisplayName)
            .AddSmtpSender(() => new SmtpClient(emailSettings.SmtpServer, emailSettings.Port)
            {
                Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password),
                EnableSsl = emailSettings.EnableSsl,
                Timeout = 30000
            });

        return services;
    }

    private static void RegisterInfrastructureServices(IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<INavisionService, NavisionService>();
        services.AddScoped<ITotpService, TotpService>();

        services.AddScoped<ISoapService, SoapService>();

        services.AddSingleton<INavisionUrlHelper, NavisionUrlHelper>();
    }


}
