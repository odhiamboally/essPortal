using EssPortal.Web.Mvc.Configurations;

using ESSPortal.Web.Mvc.Configurations;
using ESSPortal.Web.Mvc.Contracts.Implementations.AppServices;
using ESSPortal.Web.Mvc.Contracts.Implementations.Common;
using ESSPortal.Web.Mvc.Contracts.Implementations.Services;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            // Build temporary service provider to get environment
            using var tempProvider = services.BuildServiceProvider();
            var environment = tempProvider.GetService<IWebHostEnvironment>();
            var isDevelopment = environment?.IsDevelopment() ?? false;

            var apiSettings = configuration.GetSection("ApiSettings").Get<ApiSettings>() ?? throw new InvalidOperationException("ApiSettings section is missing or invalid.");

            services.AddSingleton(apiSettings);

            var baseUrl = apiSettings?.BaseUrl;
            if (!string.IsNullOrWhiteSpace(baseUrl) && !baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException($"❌ Invalid BaseUrl format: {baseUrl}");
            }

            services.AddHttpClient<IApiService, ApiService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = isDevelopment ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(100);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler()
                {
                    MaxConnectionsPerServer = 10,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                    {
                        var host = message.RequestUri?.Host?.ToLowerInvariant();

                        // Allow your API server IP address
                        return host == "localhost" ||
                               host == "127.0.0.1" ||
                               host == "10.100.80.187";
                    }
                };

                return handler;
            })
            .AddResilienceHandler("ApiResiliencePipeline", ConfigureResiliencePipeline);

            services.AddHttpContextAccessor();

            return services;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in AddApiServices: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }

    }

    public static IServiceCollection ConfigureAndValidateApiSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));

        services.PostConfigure<ApiSettings>(settings =>
        {
            if (string.IsNullOrWhiteSpace(settings.BaseUrl))
                throw new InvalidOperationException("❌ ApiSettings.BaseUrl must be configured.");

            if (!settings.BaseUrl.EndsWith("/"))
                settings.BaseUrl += "/";

            if (!Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out _))
                throw new InvalidOperationException($"❌ Invalid ApiSettings.BaseUrl format: {settings.BaseUrl}");

        });

        return services;
    }

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var securitySettings = configuration.GetSection("SecuritySettings").Get<SecuritySettings>() ?? new();
            var sessionManagementSettings = securitySettings.SessionManagement ?? new();
            var jwtSettings = LoadJwtSettings(configuration);

            services.Configure<SecuritySettings>(configuration.GetSection("SecuritySettings"));

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
            });

            services.AddSession(options =>
            {
                options.Cookie.Name = "ESS_Session";
                options.IdleTimeout = TimeSpan.FromMinutes(sessionManagementSettings.SessionTimeoutMinutes);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always; 
                options.Cookie.SameSite = SameSiteMode.Strict;

                
            });

            Console.WriteLine($"✓ Authentication configured (timeout: {sessionManagementSettings.SessionTimeoutMinutes} min)");

            return services;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in AddAuthenticationServices: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public static IServiceCollection AddApplicationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var payloadEncryptionSettings = configuration.GetSection("PayloadEncryptionSettings");
            services.Configure<PayloadEncryptionSettings>(payloadEncryptionSettings);

            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var apiSettingsSection = configuration.GetSection("ApiSettings");
            services.Configure<ApiSettings>(apiSettingsSection);

            var fileSettingsSection = configuration.GetSection("FileSettings");
            services.Configure<FileSettings>(fileSettingsSection);

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
            }

            // Configure JwtSettings (for MVC client - typically for token validation)
            var jwtSettingsSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettingsSection);

            // Validate critical configuration
            ValidateConfiguration(configuration);

            return services;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in AddApplicationConfiguration: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }

    }

    public static IServiceCollection AddMvcClientServices(this IServiceCollection services, IConfiguration configuration)
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
                .AddCheck("mvc-application", () =>
                {
                    // Check if critical services are available
                    return HealthCheckResult.Healthy("MVC Application is running");
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

            return services;
        }
        catch (Exception)
        {

            throw;
        }

    }

    public static IServiceCollection AddClientServices(this IServiceCollection services)
    {
        try
        {
            services.AddScoped<IServiceManager, ServiceManager>();
            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<ILeaveService, LeaveService>();
            services.AddScoped<ILeaveApplicationCardService, LeaveApplicationCardService>();
            services.AddScoped<ILeaveApplicationListService, LeaveApplicationListService>();
            services.AddScoped<ILeavePlannerLineService, LeavePlannerLineService>();
            services.AddScoped<ILeaveRelieverService, LeaveRelieverService>();
            services.AddScoped<ILeaveStatisticsFactboxService, LeaveStatisticsFactboxService>();
            services.AddScoped<ILeaveTypeService, LeaveTypeService>();
            services.AddScoped<IPayrollService, PayrollService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ITwoFactorService, TwoFactorService>();
            services.AddScoped<ICacheService, InMemoryCacheService>();
            services.AddScoped<IPayloadEncryptionService, PayloadEncryptionService>();


            return services;
        }
        catch (Exception)
        {

            throw;
        }

    }

    public static IServiceCollection AddLoggingServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .MinimumLevel.Information()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.With<CorrelationIdEnricher>()
                .Enrich.With<IPAddressEnricher>()
                .Enrich.WithProperty("MachineName", Environment.MachineName)
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
        if (!string.IsNullOrEmpty(returnUrl) && returnUrl != "/")
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

    private static void ConfigureResiliencePipeline(ResiliencePipelineBuilder<HttpResponseMessage> builder)
    {
        builder.AddTimeout(TimeSpan.FromMinutes(2));

        // 2. Retry strategy - handle both exceptions AND bad responses
        builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1), 
            BackoffType = DelayBackoffType.Exponential,
            MaxDelay = TimeSpan.FromSeconds(30),
            UseJitter = true,
            ShouldHandle = args =>
            {
                return args.Outcome switch
                {
                    // Handle exceptions (including HttpIOException)
                    { Exception: HttpRequestException } => PredicateResult.True(),
                    { Exception: TaskCanceledException } => PredicateResult.True(),
                    { Exception: HttpIOException } => PredicateResult.True(), 
                    { Exception: SocketException } => PredicateResult.True(),

                    { Result: HttpResponseMessage response } when
                        response.StatusCode == HttpStatusCode.TooManyRequests ||
                        response.StatusCode == HttpStatusCode.RequestTimeout ||
                        response.StatusCode == HttpStatusCode.InternalServerError ||
                        response.StatusCode == HttpStatusCode.BadGateway ||
                        response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                        response.StatusCode == HttpStatusCode.GatewayTimeout
                        => PredicateResult.True(),

                    _ => PredicateResult.False()
                };
            }
        });

        // 3. Less aggressive Circuit breaker
        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio = 0.7, // 70% failures
            MinimumThroughput = 5, 
            SamplingDuration = TimeSpan.FromSeconds(60),
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: not null } => PredicateResult.True(),
                { Result.IsSuccessStatusCode: false } => PredicateResult.True(),
                _ => PredicateResult.False()
            }

            //ShouldHandle = args =>
            //{
            //    return args.Outcome switch
            //    {
            //        { Exception: HttpRequestException } => PredicateResult.True(),
            //        { Exception: HttpIOException } => PredicateResult.True(),
            //        { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True(),
            //        _ => PredicateResult.False()
            //    };
            //}
        });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }

    private static void ValidateConfiguration(IConfiguration configuration)
    {
        string[] criticalSections = ["ApiSettings", "JwtSettings"];

        foreach (var section in criticalSections)
        {
            if (!configuration.GetSection(section).Exists())
            {
                Console.WriteLine($"⚠️ Critical section '{section}' is missing from configuration");
            }
        }
    }

    public class CorrelationIdEnricher : ILogEventEnricher
    {
        private const string CorrelationIdPropertyName = "CorrelationId";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdEnricher() : this(new HttpContextAccessor()) { }
        public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["CorrelationId"].FirstOrDefault() ?? Guid.NewGuid().ToString();
            logEvent.AddOrUpdateProperty(new LogEventProperty(CorrelationIdPropertyName, new ScalarValue(correlationId)));
        }

        private string GetCorrelationId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var correlationId = httpContext?.Request.Headers["CorrelationId"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            return correlationId;
        }
    }

    public class IPAddressEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IPAddressEnricher() : this(new HttpContextAccessor()) { }
        public IPAddressEnricher(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

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

