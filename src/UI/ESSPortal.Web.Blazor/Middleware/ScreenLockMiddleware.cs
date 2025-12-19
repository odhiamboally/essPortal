using ESSPortal.Web.Blazor.Utilities.Session;

namespace ESSPortal.Web.Blazor.Middleware;

public class ScreenLockMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ScreenLockMiddleware> _logger;

    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Auth/Lock",
        "/Auth/Unlock",
        "/Auth/TriggerLock",
        "/Auth/KeepAlive",
        "/Auth/SignIn",
        "/Auth/SignOut",
        "/Auth/SessionConfig",
        "/health"
    };

    public ScreenLockMiddleware(RequestDelegate next, ILogger<ScreenLockMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip for: excluded paths, static files, unauthenticated users
        if (ShouldSkip(context, path))
        {
            await _next(context);
            return;
        }

        // Check if screen is locked
        var isLocked = context.Session.GetString("IsLocked");
        if (isLocked == "true")
        {
            _logger.LogDebug("Request blocked - screen locked: {Path}", path);

            // Store requested URL for return after unlock
            var returnUrl = context.Request.Path + context.Request.QueryString;
            context.Session.SetString("LockReturnUrl", returnUrl);

            // Handle AJAX requests differently
            if (IsAjaxRequest(context))
            {
                context.Response.StatusCode = 423; // Locked
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"locked\":true,\"redirectUrl\":\"/Auth/Lock\"}");
                return;
            }

            context.Response.Redirect("/Auth/Lock");
            return;
        }

        await _next(context);
    }

    private bool ShouldSkip(HttpContext context, string path)
    {
        // Skip if not authenticated
        if (context.User.Identity?.IsAuthenticated != true)
            return true;

        // Skip excluded paths
        if (ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Skip static files
        if (path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/Images", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase) ||
            (path.Contains('.') && !path.EndsWith(".aspx")))
            return true;

        return false;
    }

    private bool IsAjaxRequest(HttpContext context)
    {
        return context.Request.Headers.XRequestedWith == "XMLHttpRequest" ||
               context.Request.Headers.Accept.ToString().Contains("application/json");
    }
}
