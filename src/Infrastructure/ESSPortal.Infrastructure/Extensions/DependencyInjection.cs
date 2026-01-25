using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Infrastructure.Configuration;
using ESSPortal.Infrastructure.Contracts.Implementations.Common;
using ESSPortal.Infrastructure.Contracts.Implementations.Services;
using ESSPortal.Infrastructure.Validations;
using ESSPortal.Shared.Contracts.Interfaces.Common;

using FluentValidation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Net;
using System.Net.Mail;

namespace ESSPortal.Infrastructure.Extensions;
public static class DependencyInjection
{
    public static void AddInfrastructureDI(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            ConfigureEmailSettings(services, configuration);
            ConfigureFluentEmailWithSmtp(services, configuration);
            RegisterInfrastructureServices(services);

        }
        catch (Exception)
        {
            throw;
        }

    }

    private static void ConfigureEmailSettings(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IValidator<EmailSettings>, EmailSettingsValidator>();

        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection("EmailSettings"))
            .ValidateFluentValidation()
            .ValidateOnStart();

        
    }

    private static void ConfigureFluentEmailWithSmtp(IServiceCollection services, IConfiguration configuration)
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

        services.AddSingleton<ICacheService, InMemoryCacheService>();
    }


}
