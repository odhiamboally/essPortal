using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EssPortal.Web.Mvc.Utilities.CustomAttributes;

public class EnsureAuthenticatedAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();

        if (!authService.IsAuthenticated())
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
            return;
        }

        // Ensure token is still valid
        var isValid = await authService.EnsureAuthenticatedAsync();
        if (!isValid)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("SignIn", "Auth", new { returnUrl });
            return;
        }

        await next();
    }
}
