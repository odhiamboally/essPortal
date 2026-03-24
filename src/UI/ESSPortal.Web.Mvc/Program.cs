using EssPortal.Web.Mvc.Extensions;
using EssPortal.Web.Mvc.Middleware;
using EssPortal.Web.Mvc.Utilities;

using ESSPortal.Web.Mvc.Extensions;
using ESSPortal.Web.Mvc.Utilities.Common;

using FluentValidation;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();

var headerPolicies = builder.Environment.IsDevelopment()
    ? SecurityHeaderPolicies.BuildDevelopment()
    : SecurityHeaderPolicies.BuildProduction(
        builder.Configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
        ?? throw new InvalidOperationException("ApiSettings:BaseUrl is required in production."));

var dataProtectionBuilder = builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\inetpub\wwwroot\EssPortal\publish\client\Keys"))
    .SetApplicationName("ESSPortal")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

if (OperatingSystem.IsWindows())
{
    dataProtectionBuilder.ProtectKeysWithDpapi();
}

builder.Services.ConfigureLogging(builder.Configuration);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

try
{
    builder.Services.AddClientDI(builder.Configuration);
}
catch (Exception)
{
    throw;
}

builder.Services.AddScoped<SelectiveAntiforgeryFilter>();

builder.Services.AddControllersWithViews(options =>
{
    //options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<SelectiveAntiforgeryFilter>();

}).AddJsonOptions(options =>
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

// Configure antiforgery only for MVC pages (not API)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    
});

builder.Services.AddScoped<ExceptionHandler>();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseCustomSecurityHeaders();

app.UseSecurityHeaders(headerPolicies);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        ExceptionHandler = async context =>
        {
            var exceptionHandler = context.RequestServices.GetRequiredService<ExceptionHandler>();
            await exceptionHandler.TryHandleAsync(
                context,
                context.Features.Get<IExceptionHandlerFeature>()?.Error!,
                CancellationToken.None);
        }
    });

    app.UseHsts();

}

if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("🔍 Request START: {Method} {Path} from {RemoteIp}",
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await next();
            stopwatch.Stop();

            logger.LogInformation("✅ Request COMPLETED: {Method} {Path} -> {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "❌ Request FAILED: {Method} {Path} after {ElapsedMs}ms - {Error}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                ex.Message);
            throw;
        }
    });
}

var profilePicturesPath = builder.Configuration["FileSettings:ProfilePicturesPath"];
if (string.IsNullOrWhiteSpace(profilePicturesPath))
{
    profilePicturesPath = Path.Combine("C:", "inetpub", "wwwroot", "EssPortal", "Images", "ProfilePictures");
}

if (!Directory.Exists(profilePicturesPath))
{
    Directory.CreateDirectory(profilePicturesPath);
}

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(profilePicturesPath),
    RequestPath = "/Images/ProfilePictures",
    ServeUnknownFileTypes = false,
    DefaultContentType = "application/octet-stream",
    OnPrepareResponse = ctx =>
    {
        // Add caching headers
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=604800"); // 7 days
    },
    ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".webp", "image/webp" }
    })
});

app.UseHttpsRedirection();

app.UseResponseCompression();

app.UseResponseCaching();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UsePostAuthMiddleware();

app.UseAuthorization();

app.UseCustomMiddleware();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=SignIn}/{id?}");


// Redirect route for root
app.MapGet("/", () => Results.Redirect("/Auth/SignIn"));

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting in {Environment} environment", app.Environment.EnvironmentName);

Console.WriteLine("Starting application...");
app.Run();







