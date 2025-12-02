using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Auth;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EssPortal.Web.Mvc.Controllers;

[Authorize]
public class BaseController : Controller
{
    protected readonly IServiceManager _serviceManager;
    protected readonly AppSettings _appSettings;
    protected readonly ILogger<BaseController> _logger;
    protected CurrentUserResponse? _currentUser;

    public BaseController(IServiceManager serviceManager, IOptions<AppSettings> appSettings, ILogger<BaseController> logger)
    {
        _serviceManager = serviceManager;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            _logger.LogDebug("User authenticated: {UserId}", GetCurrentUserId());

            var fullName = User.FindFirst(ClaimTypes.Name)?.Value ??
                       User.Identity.Name ??
                       "User";

            ViewBag.UserDisplayName = fullName;
            ViewBag.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.UserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            // Generate initials
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                ViewBag.UserInitials = $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            }
            else if (parts.Length == 1 && parts[0].Length >= 2)
            {
                ViewBag.UserInitials = parts[0].Substring(0, 2).ToUpper();
            }
            else
            {
                ViewBag.UserInitials = "U";
            }

            // ============================================================
            // IMPORTANT: We no longer check JWT expiry for idle timeout!
            // The Cookie Authentication middleware handles idle timeout.
            // 
            // ValidateAndRefreshTokenAsync is now ONLY for ensuring we 
            // have a valid JWT when we need to call the backend API.
            // ============================================================

            // Ensure we have a valid JWT for API calls (refresh if needed)
            // This does NOT affect user session - just API token validity
            await EnsureValidApiTokenAsync();

            // Load current user data for the request
            if (_currentUser == null)
            {
                await LoadCurrentUserAsync();
            }

            SetUserInfoInViewBag();
        }
        else
        {
            _logger.LogDebug("User not authenticated");
        }

        await base.OnActionExecutionAsync(context, next);
    }

    protected string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    protected string? GetEmployeeNumber()
    {
        return User.FindFirst("employeenumber")?.Value ?? User.FindFirst("Employeenumber")?.Value;
        
    }

    protected string? GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value;
    }

    protected string? GetUserFullName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value;
    }

    protected bool IsUserAuthenticated => User.Identity?.IsAuthenticated == true;

    protected string? CurrentUserId => GetCurrentUserId();

    protected virtual void SetUserInfoInViewBag()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            // Use YOUR existing custom claims directly - no confusion!
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            var firstNameClaim = User.FindFirst("firstname")?.Value;     
            var lastNameClaim = User.FindFirst("lastname")?.Value;       
            var employeeNumberClaim = User.FindFirst("employeenumber")?.Value;

            // Set ViewBag properties
            ViewBag.UserId = userIdClaim;
            ViewBag.UserEmail = emailClaim ?? "";
            ViewBag.EmployeeNumber = employeeNumberClaim ?? "";

            // Build display name: Always use FirstName + LastName if available
            if (!string.IsNullOrWhiteSpace(firstNameClaim) && !string.IsNullOrWhiteSpace(lastNameClaim))
            {
                ViewBag.UserDisplayName = $"{firstNameClaim} {lastNameClaim}";
                ViewBag.UserInitials = $"{firstNameClaim[0]}{lastNameClaim[0]}".ToUpperInvariant();
            }
            else if (!string.IsNullOrWhiteSpace(emailClaim))
            {
                // Fallback to email username part
                var emailName = emailClaim.Split('@')[0];
                ViewBag.UserDisplayName = emailName;
                ViewBag.UserInitials = emailName.Length >= 2
                    ? $"{emailName[0]}{emailName[1]}".ToUpperInvariant()
                    : $"{emailName[0]}U".ToUpperInvariant();
            }
            else
            {
                // Final fallback
                ViewBag.UserDisplayName = "User";
                ViewBag.UserInitials = "U";
            }

            // Profile picture (if you have this claim)
            var profilePictureClaim = User.FindFirst("profile_picture")?.Value;
            ViewBag.UserProfilePicture = profilePictureClaim;
        }
        else
        {
            // Default values for non-authenticated users
            ViewBag.UserDisplayName = "User";
            ViewBag.UserInitials = "U";
            ViewBag.UserEmail = "";
            ViewBag.UserProfilePicture = "";
        }
    }


    /// <summary>
    /// Validates model state and FluentValidation results
    /// </summary>
    protected async Task<bool> ValidateRequestAsync<T>(T request, FluentValidation.IValidator<T> validator, ModelStateDictionary modelState)
    {

        modelState.Clear();

        if (!ModelState.IsValid)
            return false;

        var validationResults = await validator.ValidateAsync(request);
        if (!validationResults.IsValid)
        {
            foreach (var error in validationResults.Errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return false;
        }

        return ModelState.IsValid;
    }

    protected IActionResult RedirectToSafeUrl(string? returnUrl, string defaultController = "Home", string defaultAction = "Index")
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(defaultAction, defaultController);
    }

    private DateTimeOffset? GetTokenExpiry(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (tokenHandler.CanReadToken(token))
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Ensures we have a valid JWT token for making API calls.
    /// This does NOT affect user session state - the cookie handles that.
    /// If the JWT is expired, we try to refresh it silently.
    /// </summary>
    private async Task EnsureValidApiTokenAsync()
    {
        try
        {
            var token = HttpContext.Request.Cookies["auth_token"];
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("No auth token found for API calls");
                return; // Cookie auth will handle the session, API calls will fail gracefully
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
            {
                _logger.LogWarning("Invalid JWT token format");
                return;
            }

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var expiry = jwtToken.ValidTo;
            var now = DateTime.UtcNow;

            // If token expires within 5 minutes, try to refresh it
            if (expiry <= now.AddMinutes(5))
            {
                _logger.LogInformation("JWT token expiring soon, attempting refresh");

                var refreshResult = await _serviceManager.AuthService.RefreshTokenAsync();
                if (refreshResult.Successful && refreshResult.Data != null)
                {
                    // Update the token cookies
                    UpdateApiTokenCookies(refreshResult.Data.AccessToken, refreshResult.Data.RefreshToken);
                    _logger.LogInformation("JWT token refreshed successfully");
                }
                else
                {
                    _logger.LogWarning("JWT token refresh failed: {Message}", refreshResult.Message);
                    // Don't kick the user out - the cookie session is still valid
                    // API calls will fail, which is appropriate
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring valid API token");
            // Don't throw - let the request continue, API calls will fail if token is bad
        }
    }

    private async Task LoadCurrentUserAsync()
    {
        try
        {
            var currentUserResponse = await _serviceManager.AuthService.GetCurrentUserAsync();
            if (currentUserResponse.Successful && currentUserResponse.Data != null)
            {
                _currentUser = currentUserResponse.Data;
            }
            else
            {
                _logger.LogWarning("Failed to get current user: {Message}", currentUserResponse.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current user");
        }
    }

    private void UpdateApiTokenCookies(string accessToken, string? refreshToken)
    {
        var accessTokenExpiry = GetTokenExpiry(accessToken) ?? DateTimeOffset.UtcNow.AddHours(1);

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = accessTokenExpiry,
            Path = "/"
        };

        Response.Cookies.Append("auth_token", accessToken, accessCookieOptions);

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            };
            Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);
        }
    }

    public static void ClearAllCookies(HttpContext http)
    {
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = http.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        };

        string[] cookies =
        {
        "auth_token",
        "refresh_token",
        "session_id",
        ".AspNetCore.Cookies",
        "ESS_Auth",
        "ESS_Session"
    };

        foreach (var c in cookies)
        {
            if (http.Request.Cookies.ContainsKey(c))
                http.Response.Cookies.Delete(c, cookieOptions);
        }
    }









}
