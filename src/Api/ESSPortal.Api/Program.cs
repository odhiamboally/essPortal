using ESSPortal.Api.Extensions;
using ESSPortal.Application.Extensions;
using ESSPortal.Infrastructure.Extensions;
using ESSPortal.Persistence.SQLServer.Extensions;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;


try
{
    Log.Information("Starting DT Sacco Staff Portal API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddLoggingServices(builder.Configuration);

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    var dataProtectionBuilder = builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\inetpub\wwwroot\EssPortal\publish\client\Keys"))
    .SetApplicationName("ESSPortal")  // MUST be identical
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

    if (OperatingSystem.IsWindows())
    {
        dataProtectionBuilder.ProtectKeysWithDpapi();
    }

    string corsPolicy = "ApiCorsPolicy";


    #region Service Registration

    builder.Services.AddApiServices(builder.Configuration);

    builder.Services.AddApplicationServices(builder.Configuration);

    builder.Services.AddInfrastructureServices(builder.Configuration);

    builder.Services.AddAuthenticationServices(builder.Configuration);

    builder.Services.AddCustomRateLimiting();

    //builder.Services.AddLocalPersistence(builder.Configuration);

    builder.Services.AddUNSaccoPersistence(builder.Configuration);

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            // Add error handling for problematic types
            options.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
            options.JsonSerializerOptions.IncludeFields = false;
        });

    // CORS for API clients
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(corsPolicy, policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

            if (allowedOrigins == null || allowedOrigins.Length == 0)
            {
                Log.Warning("No allowed origins configured for CORS policy");
                throw new InvalidOperationException("CORS policy requires at least one allowed origin to be configured.");
            }

            if (builder.Environment.IsDevelopment())
            {
                // More permissive in development
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()
                      .SetIsOriginAllowedToAllowWildcardSubdomains()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10)); 
            }
            else
            {
                // Strict in production
                policy.WithOrigins(allowedOrigins)
                      .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                      .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
                      .AllowCredentials();
            }
        });
    });

    builder.Services.AddOpenApi("v1", options =>  // Note: "v1" to bypass interceptor
    {
        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            // Remove parameters with null schema
            foreach (var param in operation.Parameters?.ToList() ?? [])
            {
                if (param.Schema == null)
                    operation.Parameters?.Remove(param);
            }

            // Remove parameter descriptions with null metadata
            for (int i = context.Description.ParameterDescriptions.Count - 1; i >= 0; i--)
            {
                if (context.Description.ParameterDescriptions[i].ModelMetadata == null)
                    context.Description.ParameterDescriptions.RemoveAt(i);
            }

            return Task.CompletedTask;
        });

        options.AddDocumentTransformer((document, context, _) =>
        {
            document.Info = new()
            {
                Title = "DT Sacco Staff Portal API",
                Version = "v1.0",
                Description = """
                UN DT Sacco Staff Portal API
                
                This API provides endpoints for:
                - Employee authentication and authorization
                - Leave Applications
                - Document management
                - User profile management
                """,
                Contact = new()
                {
                    Name = "DT Sacco IT Support",
                    Email = "support@unsacco.org",
                }
            };
            return Task.CompletedTask;
        });

        options.AddSchemaTransformer<SafeSchemaTransformer>();
    });

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy());


        
    #endregion


    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler((_ => { }));
        app.UseHsts();
    }

    app.UseSecurityHeaders();

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseCors(corsPolicy);

    app.UsePreAuthMiddleware();

    app.UseAuthentication();

    app.UsePostAuthMiddleware();

    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi()
            .CacheOutput()
            .AllowAnonymous();

        app.MapScalarApiReference(options =>
        {
            options.WithTitle("DT Sacco Staff Portal API")
                   .WithTheme(ScalarTheme.Kepler);
        });

        app.MapGet("/", () => Results.Redirect("/scalar/v1"))
           .ExcludeFromDescription()
           .AllowAnonymous();
    }

    app.MapHealthChecks("/health");

    app.MapControllers();

    Log.Information("DT Sacco Staff Portal API started successfully");

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Schema transformer to handle problematic types

public class SafeSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        try
        {
            if (schema == null) return Task.CompletedTask;

            // In OpenApi 3.0+, Properties is never null - it's pre-initialized
            //schema.Properties ??= new Dictionary<string, OpenApiSchema>();

            // Check if Properties exists before processing
            if (schema.Properties != null)
            {
                var problematicKeys = schema.Properties
                    .Where(kvp => string.IsNullOrWhiteSpace(kvp.Key) || kvp.Value == null)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in problematicKeys)
                {
                    schema.Properties.Remove(key);
                }
            }

            if (schema.AdditionalPropertiesAllowed && schema.AdditionalProperties != null)
            {
                // In OpenApi 3.0+, Type is now JsonSchemaType enum, not string
                if (schema.AdditionalProperties.Type == JsonSchemaType.Null || schema.AdditionalProperties.Type == default)
                {
                    schema.AdditionalProperties = null;
                } 

                //if (string.IsNullOrWhiteSpace(schema.AdditionalProperties.Type))
                //{
                //    schema.AdditionalProperties = null;
                //}
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error in schema transformation for type {TypeName}", context?.JsonTypeInfo?.Type?.Name);
        }

        return Task.CompletedTask;
    }
}










