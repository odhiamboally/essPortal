using System;
using System.IO;

namespace EssPortal.Web.Mvc.Middleware;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenRefreshMiddleware> _logger;

    public TokenRefreshMiddleware(RequestDelegate next, ILogger<TokenRefreshMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip token refresh for excluded paths
        if (IsExcludedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (IsStrictApiRequest(context.Request))
        {
            var token = ExtractTokenForHybrid(context.Request);
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("API request without token: {Path}", context.Request.Path);

                // Check if response hasn't started before handling
                if (!context.Response.HasStarted)
                {
                    await HandleUnauthenticated(context);
                }
                return;
            }
        }

        try
        {
            await _next(context);
        }
        catch (Exception ex) when (context.Response.HasStarted)
        {
            // Response has already started, we can't modify it
            // Log and re-throw so upstream can handle appropriately
            _logger.LogWarning(ex, "Exception occurred after response started for {Path}. Cannot modify response.", context.Request.Path);
            throw;
        }
        catch (Exception ex)
        {
            // Log unexpected exceptions
            _logger.LogError(ex, "Unhandled exception in TokenRefreshMiddleware for {Path}", context.Request.Path);
            throw;
        }
    }

    private static async Task HandleUnauthenticated(HttpContext context)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        if (IsApiRequest(context.Request))
        {
            // Return 401 for API requests with JSON response
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var response = new
            {
                error = "token_expired",
                message = "Authentication token has expired. Please login again.",
                redirect = "/Auth/SignIn"
            };
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
        else
        {
            // Redirect to login for browser requests with return URL
            var returnUrl = context.Request.Path + context.Request.QueryString;
            var loginUrl = $"/Auth/SignIn?returnUrl={Uri.EscapeDataString(returnUrl)}";
            context.Response.Redirect(loginUrl);
        }
    }

    private static string? ExtractTokenForHybrid(HttpRequest request)
    {
        // HYBRID Priority: Cookie first (primary for MVC), then Authorization header

        // 1. Check cookie first (primary for browser/MVC requests)
        if (request.Cookies.TryGetValue("auth_token", out var tokenFromCookie))
        {
            return tokenFromCookie;
        }

        // 2. Check Authorization header (for API calls or AJAX)
        if (request.Headers.ContainsKey("Authorization"))
        {
            var authHeader = request.Headers["Authorization"].ToString();
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
        }

        return string.Empty;
    }

    private static bool IsExcludedPath(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant();

        return pathValue != null && (
            // Authentication endpoints
            pathValue.StartsWith("/auth/") ||
            pathValue.StartsWith("/api/auth/") ||
            pathValue.Contains("/login") ||
            pathValue.Contains("/logout") ||
            pathValue.Contains("/register") ||
            pathValue.Contains("/signin") ||
            pathValue.Contains("/signout") ||
            pathValue.Contains("/logoutuser") ||
            // Static files
            pathValue.StartsWith("/css/") ||
            pathValue.StartsWith("/js/") ||
            pathValue.StartsWith("/images/") ||
            pathValue.StartsWith("/lib/") ||
            pathValue.StartsWith("/favicon") ||
            // System endpoints
            pathValue.StartsWith("/health") ||
            pathValue.StartsWith("/_vs/") ||
            pathValue.StartsWith("/_framework/")
        );
    }

    private static bool IsApiRequest(HttpRequest request)
    {
        return request.Path.StartsWithSegments("/api") ||
               request.Headers.Accept.Any(h => h?.Contains("application/json") == true) ||
               request.ContentType?.Contains("application/json") == true ||
               request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }

    private static bool IsStrictApiRequest(HttpRequest request)
    {
        // Only treat explicit API paths as API requests
        if (request.Path.StartsWithSegments("/api"))
        {
            return true;
        }

        // AJAX requests that explicitly expect JSON responses
        var acceptsJson = request.Headers.Accept.Any(h => h?.Contains("application/json") == true);
        var isXmlHttpRequest = request.Headers["X-Requested-With"] == "XMLHttpRequest";
        var hasJsonContent = request.ContentType?.Contains("application/json") == true;

        // Only treat as API if it's AJAX + expects JSON + sends JSON
        return isXmlHttpRequest && acceptsJson && hasJsonContent;
    }

    
}