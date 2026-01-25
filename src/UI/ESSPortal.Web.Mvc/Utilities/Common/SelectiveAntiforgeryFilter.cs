using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ESSPortal.Web.Mvc.Utilities.Common;

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
        if (request.Headers.XRequestedWith == "XMLHttpRequest")
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
