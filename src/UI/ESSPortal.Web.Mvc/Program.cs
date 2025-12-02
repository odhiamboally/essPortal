using EssPortal.Web.Mvc.Middleware;
using EssPortal.Web.Mvc.Utilities;
using ESSPortal.Web.Mvc.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);


var dataProtectionBuilder = builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\inetpub\wwwroot\EssPortal\publish\client\Keys"))
    .SetApplicationName("ESSPortal")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

if (OperatingSystem.IsWindows())
{
    dataProtectionBuilder.ProtectKeysWithDpapi();
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

try
{
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddApplicationConfiguration(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.ConfigureAndValidateApiSettings(builder.Configuration);
    builder.Services.AddAuthenticationServices(builder.Configuration);
    builder.Services.AddClientServices();
    builder.Services.AddLoggingServices(builder.Configuration);
    builder.Services.AddMvcClientServices(builder.Configuration);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error during service registration: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}

builder.Services.AddScoped<SelectiveAntiforgeryFilter>();

builder.Services.AddControllersWithViews(options =>
{
    //options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<SelectiveAntiforgeryFilter>();
});

// Configure antiforgery only for MVC pages (not API)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    
});


builder.Services.AddScoped<ExceptionHandler>();

var app = builder.Build();

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

app.Use(async (context, next) =>
{
    // Generate a unique nonce for this request
    var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    context.Items["ScriptNonce"] = nonce;

    // Security headers
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    var domain = context.Request.Host.Host;
    var scheme = context.Request.Scheme;
    var port = context.Request.Host.Port;
    var hostWithPort = port.HasValue ? $"{domain}:{port}" : domain;

    if (app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Content-Security-Policy",
            $"default-src 'self'; " +
            $"font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com https://ka-f.fontawesome.com; " +
            $"style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com https://ka-f.fontawesome.com; " +
            $"style-src-elem 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
            $"style-src-attr 'self' 'unsafe-inline'; " +
            $"script-src 'self'  'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://kit.fontawesome.com {scheme}://{hostWithPort} {scheme}://localhost:*; " +
            $"script-src-elem 'self'  https://cdn.jsdelivr.net https://cdnjs.cloudflare.com {scheme}://{hostWithPort}; " +
            $"img-src 'self' data: blob: https: http:; " +
            $"connect-src 'self' {scheme}://{hostWithPort} {scheme}://localhost:* wss://{hostWithPort} ws://localhost:* https://ka-f.fontawesome.com; " +
            $"media-src 'self'; " +
            $"object-src 'none'; " +
            $"base-uri 'self'; " +
            $"form-action 'self';"
        );
    }
    else
    {
        context.Response.Headers.Append("Content-Security-Policy",
            $"default-src 'self'; " +
            $"font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
            $"style-src 'self' 'unsafe-inline'  https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
            $"style-src-elem 'self' 'unsafe-inline'  https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
            $"style-src-attr 'self' 'unsafe-inline'; " +
            $"script-src 'self' 'unsafe-inline'  https://cdn.jsdelivr.net https://cdnjs.cloudflare.com {scheme}://{hostWithPort}; " +
            $"script-src-elem 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com {scheme}://{hostWithPort}; " +
            $"img-src 'self' data: blob: https:; " +
            $"connect-src 'self' {scheme}://{hostWithPort} wss://{hostWithPort}; " +
            $"media-src 'self'; " +
            $"object-src 'none'; " +
            $"base-uri 'self'; " +
            $"form-action 'self'; " +
            $"frame-ancestors 'none'; " 
            //$"upgrade-insecure-requests;"
        );

        context.Response.Headers.Append("Permissions-Policy",
            "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), accelerometer=(), gyroscope=()");
    }

    await next();
});


// Development debugging middleware
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

app.UseHttpsRedirection();
app.UseResponseCompression();

app.UseStaticFiles();

var profilePicturesPath = builder.Configuration["FileSettings:ProfilePicturesPath"];
if (string.IsNullOrWhiteSpace(profilePicturesPath))
{
    profilePicturesPath = Path.Combine("C:", "inetpub", "wwwroot", "EssPortal", "Images", "ProfilePictures");
}

if (!Directory.Exists(profilePicturesPath))
{
    Directory.CreateDirectory(profilePicturesPath);
}

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

app.UseResponseCaching();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
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


// ============================= SELECTIVE ANTIFORGERY FILTER =============================
public class SelectiveAntiforgeryFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "HEAD", "OPTIONS", "TRACE", "CONNECT"
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        var logger = context.HttpContext.RequestServices.GetService<ILogger<SelectiveAntiforgeryFilter>>();
        logger?.LogDebug("SelectiveAntiforgeryFilter: {Method} {Path}", request.Method, request.Path);

        if (SafeMethods.Contains(request.Method) || request.Path.StartsWithSegments("/api"))
        {
            await next();
            return;
        }

        // Skip if [IgnoreAntiforgeryToken] is present
        if (context.ActionDescriptor.EndpointMetadata.Any(em => em is IgnoreAntiforgeryTokenAttribute))
        {
            await next();
            return;
        }

        // Skip for AJAX requests without form data
        if (request.Headers.XRequestedWith == "XMLHttpRequest" )
        {
            logger?.LogDebug("Skipping antiforgery for JSON AJAX request");
            await next();
            return;
        }

        var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();

        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
            await next();
        }
        catch (AntiforgeryValidationException ex)
        {
            logger?.LogWarning("Antiforgery validation failed for {Method} {Path}: {Error}. " +
                "ContentType: {ContentType}, UserAgent: {UserAgent}",
                request.Method,
                request.Path,
                ex.Message,
                request.ContentType,
                request.Headers["User-Agent"].ToString()[..Math.Min(50, request.Headers["User-Agent"].ToString().Length)]);

            // For regular form submissions, redirect back to the form with an error
            if (request.HasFormContentType && request.Headers["X-Requested-With"] != "XMLHttpRequest")
            {
                // Set TempData error message and redirect back
                var tempDataProvider = context.HttpContext.RequestServices.GetService<ITempDataProvider>();
                var tempDataDict = tempDataProvider?.LoadTempData(context.HttpContext);
                if (tempDataDict != null)
                {
                    tempDataDict["ErrorMessage"] = "Security token expired. Please try again.";
                    tempDataProvider?.SaveTempData(context.HttpContext, tempDataDict);
                }

                // Redirect back to the referrer or login page
                var returnUrl = request.Headers["Referer"].FirstOrDefault() ?? "/Auth/SignIn";
                context.Result = new RedirectResult(returnUrl);
                return;
            }

            // For AJAX requests, return JSON error
            context.Result = new JsonResult(new
            {
                error = "Invalid antiforgery token",
                message = "Security token expired. Please refresh the page and try again."
            })
            {
                StatusCode = 400
            };
        }
    }
}




