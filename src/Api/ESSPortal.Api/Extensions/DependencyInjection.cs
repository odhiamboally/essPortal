using Asp.Versioning;
using ESSPortal.Api.Middleware;
using ESSPortal.Application.Configuration;
using ESSPortal.Domain.Entities;
using ESSPortal.Persistence.SQLServer.DataContext;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace ESSPortal.Api.Extensions;

public static class DependencyInjection
{


    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var assembly = typeof(Program).Assembly;

            services.Configure<JsonSettings>(configuration.GetSection("JsonSettings"));

            services.AddExceptionHandler<ApiExceptionHandler>();
            services.AddProblemDetails();

            // Basic API services
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddWebEncoders();
            services.AddHttpClient();
            services.AddHttpContextAccessor();

            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
                config.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);

            services.AddSingleton(jwtSettings);

            if (jwtSettings == null)
                throw new InvalidOperationException("JwtSettings not found in configuration");

            services.AddSingleton(sp =>
            {
                var jwtSettings = sp.GetRequiredService<JwtSettings>();

                return new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtSettings.GetSymmetricSecurityKey(),
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

            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                // Sign-in Requirements
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = false; // Set to true if you want additional account confirmation

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

                // Token Providers
                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
            })
            .AddEntityFrameworkStores<DBContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<AuthenticatorTokenProvider<AppUser>>(TokenOptions.DefaultAuthenticatorProvider);
            
            services.Configure<IdentityOptions>(options =>
            {
                ConfigureIdentityOptions(options);
            });


            // Authentication with JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                ConfigureJwtBearer(options, jwtSettings);
            });

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(24); // 24 hours for email tokens
            });

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

            services.AddSingleton<ILoggerFactory>(provider =>
            {
                return new SerilogLoggerFactory(Log.Logger, true);
            });


            return services;
        }
        catch (Exception)
        {

            throw;
        }

    }

    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
    {
        try
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("LoginPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 10;       // More reasonable for login
                    configureOptions.Window = TimeSpan.FromMinutes(2); // Longer window
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 3;
                });

                options.AddFixedWindowLimiter("AuthPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 5;        // 5 attempts
                    configureOptions.Window = TimeSpan.FromMinutes(1); // per minute
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 2;         // Allow 2 queued requests
                });

                options.AddFixedWindowLimiter("ApiPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 100;      // 100 requests
                    configureOptions.Window = TimeSpan.FromMinutes(1); // per minute
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 10;
                });

                options.AddFixedWindowLimiter("PasswordResetPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 3;        // Only 3 attempts
                    configureOptions.Window = TimeSpan.FromMinutes(15); // per 15 minutes
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 0;          // No queuing for security
                });

                options.AddFixedWindowLimiter("TwoFactorPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 10;       // 10 attempts
                    configureOptions.Window = TimeSpan.FromMinutes(5); // per 5 minutes
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 3;
                });

                options.AddFixedWindowLimiter("FileUploadPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 20;       // 20 uploads
                    configureOptions.Window = TimeSpan.FromHours(1); // per hour
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 5;
                });

                options.AddFixedWindowLimiter("RefreshTokenPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 10;
                    configureOptions.Window = TimeSpan.FromMinutes(1); // per hour
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 2;
                });



                // Rate limit exceeded response
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                        ? retryAfterValue.TotalSeconds.ToString()
                        : "60";

                    context.HttpContext.Response.Headers.RetryAfter = retryAfter;

                    var errorResponse = new
                    {
                        error = "rate_limit_exceeded",
                        message = $"Rate limit exceeded. Try again in {retryAfter} seconds.",
                        retryAfter
                    };

                    await context.HttpContext.Response.WriteAsync(
                        JsonSerializer.Serialize(errorResponse), token);
                };


            });
        }
        catch (Exception ex)
        {
            // Log the exception for debugging purposes
            Console.WriteLine($"Rate limit error handler failed: {ex.Message}");
            throw;
        }
        

        return services;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        try
        {
            return app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

                // API-specific headers
                context.Response.Headers.Append("X-API-Version", "1.0");
                context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");

                await next();
            });
        }
        catch (Exception)
        {

            throw;
        }
        
    }


    #region Serilog Enrichers

    public class CorrelationIdEnricher : ILogEventEnricher
    {
        private const string CorrelationIdPropertyName = "CorrelationId";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdEnricher() : this(new HttpContextAccessor()) { }

        public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var correlationId = GetCorrelationId();
            var correlationIdProperty = new LogEventProperty(CorrelationIdPropertyName, new ScalarValue(correlationId));
            logEvent.AddOrUpdateProperty(correlationIdProperty);
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

        public IPAddressEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var remoteIpAddress = httpContext?.Connection?.RemoteIpAddress;

            if (remoteIpAddress != null && !IPAddress.IsLoopback(remoteIpAddress))
            {
                var ipAddress = remoteIpAddress.ToString();
                var ipAddressProperty = propertyFactory.CreateProperty("IPAddress", ipAddress);
                logEvent.AddPropertyIfAbsent(ipAddressProperty);
            }
        }
    }

    #endregion

    #region Private Helper Methods

    private static void ConfigureIdentityOptions(IdentityOptions options)
    {
        options.ClaimsIdentity.UserNameClaimType = "Username";

        // User settings
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // Sign-in settings
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedAccount = false;
    }

    private static void ConfigureJwtBearer(JwtBearerOptions options, JwtSettings jwtSettings)
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = jwtSettings?.GetSymmetricSecurityKey(),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,

            // CUSTOM LIFETIME VALIDATOR to handle NotBefore issues
            LifetimeValidator = (notBefore, expires, token, parameters) =>
            {
                var now = DateTime.UtcNow;

                // Check expiration (required)
                if (expires.HasValue && expires.Value < now)
                {
                    return false; // Token expired
                }

                // Check not before (lenient - allow if missing or if time is close)
                if (notBefore.HasValue && notBefore.Value > now.AddMinutes(1))
                {
                    return false; // Token not yet valid (with 1 min tolerance)
                }

                return true; // Token is valid
            }
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "application/json";

                    var expirationTime = context.Exception is SecurityTokenExpiredException expiredException
                        ? expiredException.Expires.ToString("yyyy-MM-ddTHH:mm:ss")
                        : string.Empty;

                    var errorMessage = new
                    {
                        context.Response.StatusCode,
                        Message = "Token has expired.",
                        ExpirationTime = expirationTime,
                        CurrentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                        Error = context.Exception.Message
                    };

                    return context.Response.WriteAsync(JsonSerializer.Serialize(errorMessage));
                }
                else
                {
                    // Log other JWT validation errors
                    Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                    Console.WriteLine($"Exception type: {context.Exception.GetType().Name}");

                    // Check for NotBefore issues specifically
                    if (context.Exception.Message.Contains("NotBefore") || context.Exception.Message.Contains("not yet valid"))
                    {
                        Console.WriteLine("NotBefore validation issue detected");
                    }
                }

                return Task.CompletedTask;


            }
        };
    }

    #endregion


}
