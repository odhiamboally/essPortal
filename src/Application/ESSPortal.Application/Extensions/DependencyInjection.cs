using ESSPortal.Application.BackgroundServices;
using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Implementations.Caching;
using ESSPortal.Application.Contracts.Implementations.Common;
using ESSPortal.Application.Contracts.Implementations.Services;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Application.Utilities;
using ESSPortal.Application.Validations.RequestValidators.Leave;

using FluentValidation;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Polly;
using System.Net.Http.Headers;
using System.Text;

namespace ESSPortal.Application.Extensions;
public static  class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var assembly = AppDomain.CurrentDomain.Load("ESSPortal.Application");
            services.AddSingleton(assembly);
            services.AddValidatorsFromAssembly(assembly);
            //services.AddValidatorsFromAssemblyContaining<CreateLeaveApplicationRequestValidator>();

            ConfigureApplicationSettings(services, configuration);

            ConfigureHttpClients(services, configuration);

            RegisterApplicationServices(services);

            ConfigureCaching(services, configuration);

            AddBackgroundServices(services, configuration);

            services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));

            return services;
        }
        catch (Exception)
        {
            throw;
        }

    }

    private static void ConfigureApplicationSettings(IServiceCollection services, IConfiguration configuration)
    {
        var securitySettingsSection = configuration.GetSection("SecuritySettings");
        if (!securitySettingsSection.Exists())
        {
            Console.WriteLine("⚠️ SecuritySettings section not found in configuration, using defaults");
            services.Configure<SecuritySettings>(options =>
            {
                //options.SessionManagement = new SessionManagementSettings
                //{
                //    MaxConcurrentSessions = 3,
                //    SessionTimeoutMinutes = 15,
                //    SlidingExpiration = true,
                //    CleanupIntervalMinutes = 30
                //};

                options.SessionManagement = new();
            });
        }
        else
        {
            services.Configure<SecuritySettings>(securitySettingsSection);
            Console.WriteLine("✓ SecuritySettings configured successfully");
        }

        // Configure individual security sub-settings
        services.Configure<SessionManagementSettings>(configuration.GetSection("SecuritySettings:SessionManagement"));

        var backgroundServiceSection = configuration.GetSection("BackgroundServices");
        if (!backgroundServiceSection.Exists())
        {
            Console.WriteLine("⚠️ BackgroundServices section not found in configuration, using defaults");
            services.Configure<BackgroundServiceSettings>(options =>
            {
                options.EnableSessionCleanup = true;
                options.EnableSecurityEventCleanup = true;
                options.SessionCleanupIntervalMinutes = 60;
                options.SecurityEventCleanupIntervalHours = 24;
                options.RetainSecurityEventsForDays = 30;
            });
        }
        else
        {
            services.Configure<BackgroundServiceSettings>(backgroundServiceSection);
            Console.WriteLine("✓ BackgroundServiceSettings configured successfully");
        }

        var emailValidationSection = configuration.GetSection("EmailValidation");
        if (!emailValidationSection.Exists())
        {
            Console.WriteLine("⚠️ EmailValidation section not found in configuration, using defaults");
            services.Configure<EmailValidationSettings>(options =>
            {
                options.BlockPersonalDomains = true;
                options.RequireBusinessEmail = true;
                options.AllowedDomains = ["unsacco.org", "un.org"];
                options.BlockedDomains = ["temp-mail.org", "10minutemail.com", "guerrillamail.com", "mailinator.com"];
            });
        }
        else
        {
            services.Configure<EmailValidationSettings>(emailValidationSection);
            Console.WriteLine("✓ EmailValidationSettings configured successfully");
        }

        var jwtSettingsSection = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSettingsSection);

        var fileSettingsSection = configuration.GetSection("FileSettings");
        services.Configure<FileSettings>(fileSettingsSection);

        services.Configure<BCSettings>(configuration.GetSection("BCSettings"));
        services.AddOptions<BCSettings>()
            .Bind(configuration.GetSection("BCSettings"))
            .Validate(config => !string.IsNullOrWhiteSpace(config.OdataBaseUrl), "BC BaseUrl cannot be empty!")
            .ValidateOnStart();

        var jsonSettings = new JsonSettings();
        configuration.GetSection("JsonSettings").Bind(jsonSettings);
        services.AddSingleton(jsonSettings);

    }

    private static void ConfigureHttpClients(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<BasicAuthHandler>();

        services.AddHttpClient("NavisionService", (serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<BCSettings>>().Value;
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}"));

            client.BaseAddress = new Uri(settings.OdataBaseUrl ?? string.Empty);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        })
        .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1)))
        .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)))
        .AddHttpMessageHandler<BasicAuthHandler>();


        services.AddHttpClient("NavisionService.Ess", (serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<BCSettings>>().Value;
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        })
        .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1)))
        .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)))
        .AddHttpMessageHandler<BasicAuthHandler>();

    }

    private static void RegisterApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IServiceManager, ServiceManager>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IClaimsService, ClaimsService>();
        services.AddScoped<IUploadService, UploadService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ILeaveStatisticsFactboxService, LeaveStatisticsFactboxService>();
        services.AddScoped<ILeaveApplicationListService, LeaveApplicationListService>();
        services.AddScoped<ILeaveApplicationCardService, LeaveApplicationCardService>();
        services.AddScoped<ILeaveTypesService, LeaveTypesService>();
        services.AddScoped<ILeaveRelieversService, LeaveRelieversService>();
        
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IPdfGenerationService, PdfGenerationService>();
        services.AddScoped<ISessionManagementService, SessionManagementService>();
        services.AddScoped<IApprovedLeaveService, ApprovedLeaveService>();
        services.AddSingleton<IPayloadEncryptionService, PayloadEncryptionService>();

    }

    private static void ConfigureCaching(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MemoryCacheEntryOptions>(options =>
        {
            options.SlidingExpiration = TimeSpan.FromMinutes(15);
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            options.Priority = CacheItemPriority.Normal;
        });

        services.AddMemoryCache();

        services.AddSingleton<ICacheService, InMemoryCacheService>();
       
    }

    private static void AddBackgroundServices(IServiceCollection services, IConfiguration configuration)
    {
        var backgroundSettings = configuration.GetSection("BackgroundServices").Get<BackgroundServiceSettings>();

        if (backgroundSettings?.EnableSessionCleanup == true)
        {
            services.AddHostedService<SessionCleanupService>();
            Console.WriteLine("✓ SessionCleanupService registered");
        }

        if (backgroundSettings?.EnableSecurityEventCleanup == true)
        {
            services.AddHostedService<SecurityEventCleanupService>();
            Console.WriteLine("✓ SecurityEventCleanupService registered");
        }
    }
}
