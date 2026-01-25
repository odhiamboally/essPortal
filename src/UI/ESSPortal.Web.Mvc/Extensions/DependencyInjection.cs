
using EssPortal.Shared.Configurations;

using ESSPortal.Application.Extensions;
using ESSPortal.Domain.Entities;
using ESSPortal.Infrastructure.Extensions;
using ESSPortal.Persistence.SQLServer.DataContext;
using ESSPortal.Persistence.SQLServer.Extensions;
using ESSPortal.Shared.Configuration;
using ESSPortal.Shared.Contracts.Implementations.Common;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Configurations;
using ESSPortal.Web.Mvc.Contracts.Implementations.Common;
using ESSPortal.Web.Mvc.Contracts.Implementations.Services;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

using FluentValidation;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace EssPortal.Web.Mvc.Utilities;

public static class DependencyInjection
{
    public static IServiceCollection AddClientDI(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureMvcClientServices(services);
        ConfigureLogging(services, configuration);
        ConfigureAuthentication(services, configuration);
        ConfigureSettings(services, configuration);
        ConfigureClientServices(services);

        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddHttpContextAccessor();

        services.AddApplicationDI(configuration);
        services.AddInfrastructureDI(configuration);
        services.AddLocalPersistenceDI(configuration);
        //services.AddPersistenceDI(configuration);
        
        

        return services;

    }

    private static void ConfigureMvcClientServices(this IServiceCollection services)
    {
        try
        {
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1000; // Limit cache size
                options.CompactionPercentage = 0.25; // Remove 25% when limit reached
            });

            // Response compression for better performance
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();

                // Compress these MIME types
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([
                    "application/json",
                    "text/json"
                ]);
            });

            // Response caching for static content and pages
            services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 1024 * 1024; // 1MB max
                options.UseCaseSensitivePaths = false;
            });

            // Health checks for the MVC application itself (not external APIs)
            services.AddHealthChecks()
                .AddCheck("mvc-client", () =>
                {
                    // Check if critical services are available
                    return HealthCheckResult.Healthy("MVC Client is running");
                })
                .AddCheck("memory", () =>
                {
                    // Simple memory check
                    var allocated = GC.GetTotalMemory(false);
                    var threshold = 1024 * 1024 * 500; // 500MB threshold

                    return allocated < threshold
                        ? HealthCheckResult.Healthy($"Memory usage: {allocated / 1024 / 1024}MB")
                        : HealthCheckResult.Degraded($"High memory usage: {allocated / 1024 / 1024}MB");
                });

        }
        catch (Exception)
        {

            throw;
        }

    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var securitySettings = configuration.GetSection("SecuritySettings").Get<SecuritySettings>() ?? new();
            var sessionManagementSettings = securitySettings.SessionManagement ?? new();
            var jwtSettings = LoadJwtSettings(configuration);

            services.Configure<SecuritySettings>(configuration.GetSection("SecuritySettings"));

            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                // Sign-in Requirements
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = false;

                // Password Requirements
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 1;

                // Lockout Settings
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // User Settings
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // Claims Identity
                options.ClaimsIdentity.UserNameClaimType = "Username";
            })
            .AddEntityFrameworkStores<DBContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<AuthenticatorTokenProvider<AppUser>>(TokenOptions.DefaultAuthenticatorProvider);

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(24); // 24 hours for email tokens
            });

            services.AddSingleton(sp =>
            {
                var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey ?? throw new InvalidOperationException("JWT SecretKey is required"));

                return new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkew),
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                };
            });


            // Build temporary service provider to get environment
            using var tempProvider = services.BuildServiceProvider();
            var environment = tempProvider.GetService<IWebHostEnvironment>();
            var isDevelopment = environment?.IsDevelopment() ?? false;

            // Configure authentication schemes
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                ConfigureCookieAuthentication(options, sessionManagementSettings, isDevelopment);
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                ConfigureJwtAuthentication(options, jwtSettings, isDevelopment);
            });

            services.AddAuthorization(options =>
            {
                // Default policy uses cookies (for MVC)
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
                    .Build();

                // API policy uses JWT
                options.AddPolicy("ApiPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                });

            }).AddAuthorizationBuilder();

            services.AddSession(options =>
            {
                options.Cookie.Name = "ESS_Session";
                options.IdleTimeout = TimeSpan.FromMinutes(sessionManagementSettings.SessionTimeoutMinutes);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always; 

                options.Cookie.SameSite = SameSiteMode.Strict;

                
            });

        }
        catch (Exception)
        {
            
            throw;
        }
    }

    private static void ConfigureSettings(IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var payloadEncryptionSettings = configuration.GetSection("PayloadEncryptionSettings");
            services.Configure<PayloadEncryptionSettings>(payloadEncryptionSettings);

            var fileSettingsSection = configuration.GetSection("FileSettings");
            services.Configure<FileSettings>(fileSettingsSection);

            var emailValidationSection = configuration.GetSection("EmailValidation");
            if (!emailValidationSection.Exists())
            {
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
            }

            // Configure JwtSettings (for MVC client - typically for token validation)
            var jwtSettingsSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettingsSection);

            // Validate critical configuration
            ValidateConfiguration(configuration);

        }
        catch (Exception)
        {
            throw;
        }

    }

    private static void ConfigureClientServices(IServiceCollection services)
    {
        try
        {
            services.AddScoped<IClientServiceManager, ClientServiceManager>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IPayloadEncryptionService, PayloadEncryptionService>();


            
        }
        catch (Exception)
        {

            throw;
        }

    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .MinimumLevel.Information()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Serilog", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.With<CorrelationIdEnricher>()
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.With<IPAddressEnricher>()
                .WriteTo.Async(s => s.Console(new CompactJsonFormatter()))
                .WriteTo.Async(s => s.File(
                    path: configuration["Serilog:WriteTo:1:Args:path"]!,
                    rollingInterval: RollingInterval.Day,
                    formatter: new JsonFormatter()))
                .CreateLogger();

            services.AddSingleton<ILoggerFactory>(_ => new SerilogLoggerFactory(Log.Logger, true));

            return services;
        }
        catch (Exception)
        {

            throw;
        }

    }

    private static void ConfigureCookieAuthentication(CookieAuthenticationOptions options, SessionManagementSettings sessionSettings, bool isDevelopment)
    {
        // Cookie identity
        options.Cookie.Name = "ESS_Auth";
        options.LoginPath = "/Auth/SignIn";
        options.LogoutPath = "/Auth/SignOut";
        options.AccessDeniedPath = "/Auth/AccessDenied";

        // Session timeout with sliding expiration
        options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionSettings.SessionTimeoutMinutes);
        options.SlidingExpiration = sessionSettings.SlidingExpiration;

        // Cookie security
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        // Event handlers
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = context => HandleValidatePrincipal(context, sessionSettings),
            OnRedirectToLogin = HandleRedirectToLogin,
            OnRedirectToAccessDenied = HandleRedirectToAccessDenied,
            OnSigningOut = HandleSigningOut
        };
    }

    private static void ConfigureJwtAuthentication(JwtBearerOptions options, JwtSettings jwtSettings, bool isDevelopment)
    {
        options.RequireHttpsMetadata = !isDevelopment;
        options.SaveToken = false;
        options.IncludeErrorDetails = isDevelopment;

        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey ?? throw new InvalidOperationException("JWT SecretKey is required"));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkew),
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            LifetimeValidator = ValidateTokenLifetime
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = ExtractTokenFromRequest(context.Request);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                if (IsApiRequest(context.Request))
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "unauthorized", message = "Valid JWT token required" }));
                        
                }

                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    }

    private static Task HandleValidatePrincipal(CookieValidatePrincipalContext context, SessionManagementSettings settings)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<CookieAuthenticationEvents>>();
        var expiresUtc = context.Properties.ExpiresUtc;

        if (expiresUtc.HasValue)
        {
            var remaining = expiresUtc.Value - DateTimeOffset.UtcNow;
            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            logger?.LogDebug("Session for {UserId}: {Remaining:mm\\:ss} remaining", userId, remaining);
        }

        //ToDo: Can sync with database sessions:
        // var sessionService = context.HttpContext.RequestServices.GetService<ISessionManagementService>();
        // var sessionId = context.Principal?.FindFirst("session_id")?.Value;
        // if (sessionId != null)
        // {
        //     var isValid = await sessionService.IsSessionValidAsync(sessionId, userId);
        //     if (!isValid.Successful)
        //     {
        //         context.RejectPrincipal();
        //         await context.HttpContext.SignOutAsync();
        //         return;
        //     }
        // }

        // Force cookie renewal on each request (sliding expiration)
        context.ShouldRenew = true;

        return Task.CompletedTask;
    }

    private static Task HandleRedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<CookieAuthenticationEvents>>();

        if (IsApiRequest(context.Request))
        {
            logger?.LogDebug("API request unauthorized, returning 401");
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(new
            {
                error = "session_expired",
                message = "Your session has expired. Please sign in again. DI",
                authenticated = false
            });
        }

        // Browser request - redirect with session expired flag
        var returnUrl = context.Request.Path + context.Request.QueryString;
        var redirectUrl = $"{context.RedirectUri}&sessionExpired=true";

        // If returnUrl isn't already in RedirectUri, add it
        if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl != "/")
        {
            redirectUrl = $"/Auth/SignIn?returnUrl={Uri.EscapeDataString(returnUrl)}&sessionExpired=true";
                
        }
        else
        {
            redirectUrl = "/Auth/SignIn?sessionExpired=true";
        }

        logger?.LogDebug("Browser unauthorized, redirecting to: {Url}", redirectUrl);
        context.Response.Redirect(redirectUrl);
        return Task.CompletedTask;
    }

    private static Task HandleRedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        if (IsApiRequest(context.Request))
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(new
            {
                error = "access_denied",
                message = "You do not have permission to access this resource."
            });
        }

        return Task.CompletedTask;
    }

    private static Task HandleSigningOut(CookieSigningOutContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<CookieAuthenticationEvents>>();
        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        logger?.LogInformation("User signing out: {UserId}", userId);

        // Clear JWT cookies
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            Secure = context.HttpContext.Request.IsHttps,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        };

        context.HttpContext.Response.Cookies.Delete("auth_token", cookieOptions);
        context.HttpContext.Response.Cookies.Delete("refresh_token", cookieOptions);

        //ToDo: If you have database sessions, end them here:
        // var sessionService = context.HttpContext.RequestServices.GetService<ISessionManagementService>();
        // var sessionId = context.HttpContext.User.FindFirst("session_id")?.Value;
        // if (sessionId != null)
        // {
        //     await sessionService.EndSessionAsync(sessionId);
        // }

        return Task.CompletedTask;
    }

    private static bool ValidateTokenLifetime(DateTime? notBefore, DateTime? expires, SecurityToken token, TokenValidationParameters parameters)
    {
        var now = DateTime.UtcNow;

        if (expires.HasValue && expires.Value < now)
            return false;

        if (notBefore.HasValue && notBefore.Value > now.AddMinutes(1))
            return false;

        return true;
    }

    private static JwtSettings LoadJwtSettings(IConfiguration configuration)
    {
        var section = configuration.GetSection("JwtSettings");
        return new JwtSettings
        {
            SecretKey = section["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is required"),
            Issuer = section["Issuer"] ?? "ESSPortal",
            Audience = section["Audience"] ?? "ESSPortal",
            AccessTokenExpiryMinutes = int.Parse(section["AccessTokenExpiryMinutes"] ?? "60"),
            RefreshTokenExpiryHours = int.Parse(section["RefreshTokenExpiryHours"] ?? "8"),
            ClockSkew = int.Parse(section["ClockSkew"] ?? "1")
        };
    }

    private static string? ExtractTokenFromRequest(HttpRequest request)
    {
        // Authorization header (standard)
        if (request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var headerValue = authHeader.ToString();
            if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return headerValue["Bearer ".Length..].Trim();
            }
        }

        // Fallback to cookie (for hybrid scenarios)
        if (request.Cookies.TryGetValue("auth_token", out var tokenFromCookie))
        {
            return tokenFromCookie;
        }

        return null;
    }

    private static bool IsApiRequest(HttpRequest request)
    {
        // Explicit API path takes precedence
        if (request.Path.StartsWithSegments("/api"))
            return true;

        // AJAX requests (XMLHttpRequest header)
        if (request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return true;

        // Has Authorization header (Bearer token)
        if (request.Headers.ContainsKey("Authorization"))
            return true;

        // Content-Type is JSON (actual JSON POST body)
        if (request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
            return true;

        return false;
    }

    private static void ValidateConfiguration(IConfiguration configuration)
    {
        string[] criticalSections = ["ApiSettings", "JwtSettings"];

        foreach (var section in criticalSections)
        {
            if (!configuration.GetSection(section).Exists())
            {
                // ToDo: Log
            }
        }
    }

    private class CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
    {
        private const string CorrelationIdPropertyName = "CorrelationId";
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public CorrelationIdEnricher() : this(new HttpContextAccessor()) { }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["CorrelationId"].FirstOrDefault() ?? Guid.CreateVersion7().ToString();

            logEvent.AddOrUpdateProperty(new LogEventProperty(CorrelationIdPropertyName, new ScalarValue(correlationId)));
        }

    }

    private class IPAddressEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public IPAddressEnricher() : this(new HttpContextAccessor()) { }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress;
            if (ip != null && !IPAddress.IsLoopback(ip))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IPAddress", ip.ToString()));
            }

        }

    }



}

