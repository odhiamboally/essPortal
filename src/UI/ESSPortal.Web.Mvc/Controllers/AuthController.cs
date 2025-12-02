using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Auth;
using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Utilities.Constants;
using EssPortal.Web.Mvc.ViewModels.Auth;

using ESSPortal.Web.Mvc.Configurations;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Dtos.Auth;
using ESSPortal.Web.Mvc.Extensions;
using ESSPortal.Web.Mvc.Mappings;
using ESSPortal.Web.Mvc.Validations.RequestValidators.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EssPortal.Web.Mvc.Controllers;


public class AuthController : BaseController
{
    private readonly EmailValidationSettings _emailValidationSettings;
    private readonly SecuritySettings _securitySettings;

    private readonly IWebHostEnvironment _environment;

    private const string SessionKey_2FA_UserId = "2FA_UserId";
    private const string SessionKey_2FA_Provider = "2FA_Provider";
    private const string SessionKey_2FA_SentAt = "2FA_SentAt";
    private const string SessionKey_2FA_ExpiresAt = "2FA_ExpiresAt";
    private const string SessionKey_UserProfile = "UserProfile";
    private const string SessionKey_UserInfo = "UserInfo";

    public AuthController(
        IServiceManager serviceManager, 
        IOptions<AppSettings> appSettings,
        IOptions<SecuritySettings> securitySettings,
        ILogger<AuthController> logger, 
        IOptions<EmailValidationSettings> emailValidationSettings, IWebHostEnvironment environment)
        : base(serviceManager, appSettings, logger)
    {
        
        _emailValidationSettings = emailValidationSettings.Value;
        _securitySettings = securitySettings.Value;
        _environment = environment;
    }



    [HttpGet]
    [Route("Auth/SessionConfig")]
    [AllowAnonymous]
    public IActionResult SessionConfig()
    {
        try
        {
            // Return configuration for JavaScript
            return Json(new
            {
                sessionTimeoutMinutes = _securitySettings.SessionManagement.SessionTimeoutMinutes,
                idleTimeoutMinutes = _securitySettings.SessionManagement.IdleTimeoutMinutes,
                warningThresholdSeconds = 30,  
                criticalThresholdSeconds = 10, 
                keepAliveIntervalSeconds = 30,
                checkIntervalSeconds = 15,
                useLockScreen = _securitySettings.SessionManagement.UseLockScreen,
                debugMode = _environment.IsDevelopment()
            });


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session config");
            return Json(new { error = "Failed to load configuration" });
        }
    }

    [HttpGet]
    [Route("Auth/SessionStatus")]
    [Authorize]  // Changed from AllowAnonymous - must be authenticated
    public async Task<IActionResult> SessionStatus()
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new
                {
                    authenticated = false,
                    message = "Not authenticated"
                });
            }

            // Get the actual cookie expiration time
            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            int cookieTimeRemaining = 0;
            if (authResult.Succeeded && authResult.Properties?.ExpiresUtc != null)
            {
                var expiresUtc = authResult.Properties.ExpiresUtc.Value;
                cookieTimeRemaining = Math.Max(0, (int)(expiresUtc - DateTimeOffset.UtcNow).TotalSeconds);
            }
            
            return Json(new
            {
                authenticated = true,
                cookieTimeRemainingSeconds = cookieTimeRemaining,
                warningThresholdSeconds = 30,
                userId = GetCurrentUserId(),
                timestamp = DateTimeOffset.UtcNow.ToString("O")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session status");
            return Json(new
            {
                authenticated = false,
                error = "Error checking session status"
            });
        }
    }

    [HttpPost]
    [Route("Auth/KeepAlive")]
    [Authorize]
    public async Task<IActionResult> KeepAlive()
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new
                {
                    success = false,
                    message = "Not authenticated",
                    shouldRedirect = true,
                    redirectUrl = "/Auth/SignIn?sessionExpired=true"
                });
            }

            var userId = GetCurrentUserId();
            _logger.LogDebug("Keep-alive request from user: {UserId}", userId);

            // Re-authenticate to refresh the cookie expiration
            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (authResult.Succeeded && authResult.Principal != null)
            {
                var properties = authResult.Properties ?? new AuthenticationProperties();

                properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(_securitySettings.SessionManagement.SessionTimeoutMinutes);

                properties.IssuedUtc = DateTimeOffset.UtcNow;
                properties.AllowRefresh = true;

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal,
                    properties);

                var apiResult = await _serviceManager.AuthService.KeepAliveAsync();
                if (!apiResult.Successful)
                {
                    // Database session was invalidated (admin ended it, concurrent limit, etc.)
                    _logger.LogWarning("Keep-alive failed: {Message}", apiResult.Message);

                    // Sign out the user
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    return Json(new
                    {
                        success = false,
                        message = apiResult.Message,
                        requiresLogin = true
                    });
                }

                _logger.LogDebug("Keep-alive: session extended for user {UserId}", GetCurrentUserId());

                return Json(new
                {
                    success = true,
                    message = "Session extended",
                    timestamp = DateTimeOffset.UtcNow.ToString("O")

                });
            }

            return Json(new { success = false, message = "Could not refresh session" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KeepAlive failed");
            return Json(new
            {
                success = false,
                message = "Keep-alive failed",
                error = ex.Message
            });
        }
    }

    [HttpPost]
    [Route("Auth/ExtendSession")]
    [Authorize]
    public async Task<IActionResult> ExtendSession()
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            var userId = GetCurrentUserId();
            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (authResult.Succeeded && authResult.Principal != null)
            {
                var properties = authResult.Properties ?? new AuthenticationProperties();
                properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(_securitySettings.SessionManagement.SessionTimeoutMinutes);
                properties.IssuedUtc = DateTimeOffset.UtcNow;
                properties.AllowRefresh = true;

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal,
                    properties);

                // Clear lock screen cookie
                //ClearLockScreenCookie();

                _logger.LogInformation("Session extended explicitly for user: {UserId}", userId);

                return Json(new
                {
                    success = true,
                    message = "Session extended successfully",
                    extendedBy = 300,
                    newExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5).ToString("O"),
                    timestamp = DateTimeOffset.UtcNow.ToString("O")
                });
            }

            return Json(new { success = false, message = "Could not extend session" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending session");
            return Json(new { success = false, message = "Failed to extend session" });
        }
    }

    [HttpPost]
    [Route("Auth/Unlock")]
    [Authorize]
    public async Task<IActionResult> Unlock([FromBody] UnlockRequest unlockRequest)
    {
        try
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new
                {
                    success = false,
                    message = "Session expired. Please sign in again.",
                    requiresLogin = true
                });
            }

            if (string.IsNullOrWhiteSpace(unlockRequest?.Password))
            {
                return Json(new { success = false, message = "Password is required." });
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Json(new { success = false, message = "User not found.", requiresLogin = true });
            }

            var employeeNumber = GetEmployeeNumber();
            if (employeeNumber == null) 
            {
                return Json(new { success = false, message = "Employee number not found.", requiresLogin = true });
            }
            
            var userEmail = GetUserEmail();
            if (userEmail == null) 
            {
                return Json(new { success = false, message = "User email not found.", requiresLogin = true });
            }

            _logger.LogInformation("Unlock attempt for user: {UserId}", userId);

            unlockRequest = unlockRequest with 
            { 
                Email = userEmail, 
                EmployeeNumber = employeeNumber 
            };


            var unlockSessionResult = await _serviceManager.AuthService.UnlockSessionAsync(unlockRequest);

            if (!unlockSessionResult.Successful)
            {
                // Check if account is locked
                if (unlockSessionResult.Data?.AccountLocked == true)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Account locked due to too many failed attempts",
                        locked = true
                    });
                }

                // Check if session expired
                if (unlockSessionResult.Data?.SessionExpired == true)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Session expired. Please sign in again.",
                        requiresLogin = true
                    });
                }

                var accountLocked = unlockSessionResult.Message?.Contains("locked", StringComparison.OrdinalIgnoreCase) ?? false;

                return Json(new
                {
                    success = false,
                    message = accountLocked ? "Account locked due to too many failed attempts." : "Invalid password.",
                    locked = accountLocked
                });
            }

            // Password verified - extend session
            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (authResult.Succeeded && authResult.Principal != null)
            {
                var properties = authResult.Properties ?? new AuthenticationProperties();
                properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(_securitySettings.SessionManagement.SessionTimeoutMinutes);
                properties.IssuedUtc = DateTimeOffset.UtcNow;
                properties.AllowRefresh = true;

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal,
                    properties);

                _logger.LogInformation("Session unlocked for user: {UserId}", userId);

                return Json(new
                {
                    success = true,
                    message = "Session unlocked successfully.",
                });
            }

            return Json(new { success = false, message = "Could not extend session.", requiresLogin = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unlock error");
            return Json(new { success = false, message = "An error occurred. Please try again." });
        }
    }

    [AllowAnonymous]
    public IActionResult Register()
    {
        if (IsUserAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterEmployeeRequest());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterEmployeeRequest request)
    {
        var validator = new RegisterEmployeeRequestValidator(Options.Create(_emailValidationSettings));
        if (!await ValidateRequestAsync(request, validator, ModelState))
        {
            // For validation errors, use ViewBag since we're returning the view directly
            ViewBag.ErrorMessage = "Please correct the validation errors and try again.";
            return View(request);
        }

        try
        {
            request = request with
            {
                EmployeeNumber = request.EmployeeNumber?.Trim().ToUpperInvariant() ?? string.Empty,
                
            };
            
            var response = await _serviceManager.AuthService.RegisterEmployeeAsync(request);

            if (!response.Successful)
            {
                _logger.LogWarning("Registration failed for employee: {EmployeeNumber}. Message: {Message}",
                request.EmployeeNumber, response.Message);

                ModelState.AddModelError(string.Empty, response.Message ?? "Registration failed. Please try again.");

                ViewBag.ErrorMessage = response.Message ?? "Registration failed. Please try again.";

                return View(request);
            }

            _logger.LogInformation("User registration successful for employee: {EmployeeNumber} from IP: {IpAddress}",
            request.EmployeeNumber, HttpContext.Connection.RemoteIpAddress);

            this.ToastActivitySuccess("user_registered", "Account created successfully! Please check your email to confirm your account.");

            return RedirectToAction(nameof(SignIn));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for employee: {EmployeeNumber} from IP: {IpAddress}",
            request.EmployeeNumber, HttpContext.Connection.RemoteIpAddress);

            ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");

            // Exception error - use ViewBag since we're returning the view
            ViewBag.ErrorMessage = "Registration failed. Please try again.";

            return View(request);
        }
    }

    [AllowAnonymous]
    public IActionResult SignIn(string? returnUrl = null, bool sessionExpired = false)
    {
        if (IsUserAuthenticated)
        {
            return RedirectToSafeUrl(returnUrl);
        }

        // Only show session expired message if user was actually authenticated before
        if (sessionExpired)
        {
            // Check if there are any authentication cookies or tokens present
            var hasAuthCookie = Request.Cookies.ContainsKey("ESS_Auth") ||
                               Request.Cookies.ContainsKey("auth_token") ||
                               Request.Cookies.ContainsKey(".AspNetCore.Cookies");

            if (hasAuthCookie)
            {
                this.ToastSessionInfo("Your session has expired due to inactivity. Please sign in again.");
                _logger.LogInformation("User redirected to sign in due to session expiry");

                try
                {
                    HttpContext.Session.Clear();
                }
                catch (InvalidOperationException)
                {
                    // Session not available, ignore
                }
            }
            // If no auth cookies present, this is likely a fresh start - don't show the message
        }

        ViewBag.SessionExpired = sessionExpired;
        ViewBag.ReturnUrl = returnUrl;

        return View(new LoginRequest { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            var response = await _serviceManager.AuthService.SignInAsync(request);

            if (!response.Successful || string.IsNullOrWhiteSpace(response.Data?.Token))
            {
                _logger.LogWarning("Login failed for employee: {EmployeeNumber} from IP: {IpAddress}. Reason: {Reason}",
                    request.EmployeeNumber, HttpContext.Connection.RemoteIpAddress, response.Message);

                var errorResult = HandleLoginError(response, request);
                if (errorResult != null) return errorResult;

                this.ToastError(response.Message ?? "Something went wrong.");
                return View(request);
            }

            var loginData = response.Data!;
            var sessionId = response.SessionId ?? string.Empty;

            if (loginData.Requires2FA)
            {
                return await Handle2FARequirement(loginData, request.ReturnUrl);
            }

            // Successful login
            await SetAuthenticationTokenAsync(
                loginData.Token,
                loginData.RefreshToken,
                loginData.UserClaims?.ToClaimList());

            HttpContext.Session.SetString("LastActivity", DateTimeOffset.UtcNow.ToString());

            if (loginData.UserInfo != null)
            {
                HttpContext.Session.SetString(SessionKey_UserInfo, JsonSerializer.Serialize(loginData.UserInfo));
                await HttpContext.Session.CommitAsync();

            }

            _logger.LogInformation("Login successful for employee: {EmployeeNumber} from IP: {IpAddress}",
                request.EmployeeNumber, HttpContext.Connection.RemoteIpAddress);

            this.ToastAuthSuccess("Welcome back! You have successfully signed in.");

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for employee: {EmployeeNumber} from IP: {IpAddress}",
                request.EmployeeNumber, HttpContext.Connection.RemoteIpAddress);

            this.ToastError("An unexpected error occurred. Please try again.");
            return View(request);
        }
    }

    #region Two-Factor Authentication


    [AllowAnonymous]
    public async Task<IActionResult> TwoFactorLogin(string? returnUrl = null)
    {
        var userId = HttpContext.Session.GetString(SessionKey_2FA_UserId);
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("2FA login attempted without valid session from IP: {IpAddress}",
                HttpContext.Connection.RemoteIpAddress);

            this.ToastError("Two-factor authentication session expired. Please login again.");
            return RedirectToAction(nameof(SignIn));
        }

        try
        {
            var providersResponse = await _serviceManager.AuthService.Get2FAProvidersAsync(new Get2FAProviderRequest(userId));

            if (!providersResponse.Successful || providersResponse.Data?.Providers == null || !providersResponse.Data.Providers.Any())
            {
                _logger.LogWarning("No 2FA providers available for user: {UserId}", userId);
                this.ToastError("No two-factor authentication methods available.");
                return RedirectToAction(nameof(SignIn));
            }

            var providers = providersResponse.Data.Providers.Where(p => p.IsEnabled).ToList();
            if (!providers.Any())
            {
                _logger.LogWarning("No enabled 2FA providers for user: {UserId}", userId);
                this.ToastError("No two-factor authentication methods configured. Please contact IT support.");
                return RedirectToAction(nameof(SignIn));
            }

            // Find the best default provider
            var defaultProvider = providers.FirstOrDefault(p => p.IsDefault && p.IsEnabled) ??
                                 providers.FirstOrDefault(p => p.Value == "Authenticator" && p.IsEnabled) ??
                                 providers.FirstOrDefault(p => p.IsEnabled);

            if (defaultProvider == null)
            {
                _logger.LogWarning("No suitable default provider found for user: {UserId}", userId);
                this.ToastError("Two-factor authentication configuration error. Please contact IT support.");
                return RedirectToAction(nameof(SignIn));
            }

            bool isTotpProvider = IsTotpProvider(defaultProvider.Value);

            // Check if TOTP provider is properly configured
            if (isTotpProvider && !defaultProvider.IsEnabled)
            {
                this.ToastInfo("Please set up your authenticator app first.");
                return RedirectToAction("Setup", "TwoFactor", new { returnUrl });
            }

            var viewModel = new TwoFactorLoginViewModel
            {
                UserId = userId,
                ReturnUrl = returnUrl,
                Provider = defaultProvider.Value,
                ProviderDisplayName = defaultProvider.DisplayName,
                RequiresCodeSending = !isTotpProvider, // TOTP doesn't require sending
                TwoFactorProviders = providers,
                Providers = providers.Select(p => new SelectListItem
                {
                    Value = p.Value,
                    Text = p.DisplayName,
                    Selected = p.Value == defaultProvider.Value
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading 2FA login for user: {UserId}", userId);
            this.ToastError("Failed to load two-factor authentication options.");
            return RedirectToAction(nameof(SignIn));
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Send2FACode(Send2FACodeRequest request)
    {
        if (!RequiresCodeSending(request.SelectedProvider))
        {
            return Json(new { successful = false, message = "This provider does not require code sending." });
        }

        var validator = new Send2FACodeRequestValidator();
        if (!await ValidateRequestAsync(request, validator, ModelState))
        {
            _logger.LogWarning("2FA code request validation failed for user: {UserId} via {Provider}. Errors: {Errors}",
                request.UserId, request.SelectedProvider, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            this.ToastError("Validation failed. Please check your input and try again.");

            return Json(new { successful = false, message = "Validation failed", errors = ModelState });
        }

        try
        {
            var response = await _serviceManager.AuthService.Send2FACodeAsync(request);

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to send 2FA code for user: {UserId} via {Provider}. Reason: {Reason}",
                    request.UserId, request.SelectedProvider, response.Message);

                this.ToastError(response.Message ?? "Failed to send verification code. Please try again.");

                return Json(new { successful = false, message = response.Message ?? "Failed to send verification code." });
            }

            // Store verification session data using constants
            HttpContext.Session.SetString(SessionKey_2FA_UserId, response.Data!.UserId);
            HttpContext.Session.SetString(SessionKey_2FA_Provider, response.Data.Provider);
            HttpContext.Session.SetString(SessionKey_2FA_SentAt, response.Data.SentAt.ToString());
            HttpContext.Session.SetString(SessionKey_2FA_ExpiresAt, response.Data.ExpiresAt.ToString());

            await HttpContext.Session.CommitAsync();

            _logger.LogInformation("2FA code sent successfully for user: {UserId} via {Provider} to {Destination}",
                response.Data.UserId, request.SelectedProvider, response.Data.MaskedDestination);

            this.ToastAuthSuccess("2fa_code_sent", $"A verification code has been sent to your selected method. Please check your {GetProviderDisplayName(request.SelectedProvider ?? string.Empty)}.");

            return Json(new
            {
                successful = true,
                message = "Verification code sent successfully.",
                maskedDestination = response.Data.MaskedDestination
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending 2FA code for user {UserId} via {Provider}", request.UserId, request.SelectedProvider);
            this.ToastError("Failed to send verification code. Please try again later or contact IT support.");
            ModelState.AddModelError(string.Empty, "Failed to send verification code. Please try again.");
            return await ReloadSend2FACodeView(request);
        }
    }

    [AllowAnonymous]
    public async Task<IActionResult> Resend2FACode(string? returnUrl = null)
    {
        var userId = HttpContext.Session.GetString(SessionKey_2FA_UserId);
        var provider = HttpContext.Session.GetString(SessionKey_2FA_Provider);

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(provider))
        {
            this.ToastError("No active verification session found.");
            return RedirectToAction(nameof(SignIn));
        }

        var request = new Send2FACodeRequest
        {
            UserId = userId,
            SelectedProvider = provider,
            ReturnUrl = returnUrl,
            ForceResend = true
        };

        return await Send2FACode(request);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Verify2FACode(TwoFactorLoginViewModel model)
    {
        var request = new Verify2FACodeRequest
        {
            UserId = model.UserId?.Trim() ?? string.Empty,
            Provider = model.Provider?.Trim() ?? string.Empty,
            ProviderDisplayName = model.ProviderDisplayName?.Trim() ?? string.Empty,
            Code = model.Code?.Trim() ?? string.Empty,
            RememberMe = model.RememberMe,
            RememberDevice = model.RememberDevice,
            RememberBrowser = model.RememberBrowser,
            DeviceFingerprint = model.DeviceFingerprint?.Trim(),
            ReturnUrl = model.ReturnUrl?.Trim()
        };

        if (request.ProviderDisplayName?.Contains("Authenticator") == true)
        {
            request = request with { Provider = "Authenticator" };
        }

        var validator = new Verify2FACodeRequestValidator();
        if (!await ValidateRequestAsync(request, validator, ModelState))
        {
            this.ToastError("Validation failed. Please check your input and try again.");

            _logger.LogWarning("2FA code verification validation failed for user: {UserId}. Errors: {Errors}",
                request.UserId, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            return RedirectToAction("TwoFactorLogin", new { request.ReturnUrl });
        }

        try
        {
            // Verify session data matches request
            var userId = HttpContext.Session.GetString(SessionKey_2FA_UserId);
            var sessionProvider = HttpContext.Session.GetString(SessionKey_2FA_Provider);

            if (userId != request.UserId)
            {
                _logger.LogWarning("2FA session mismatch for user: {RequestUserId}, session user: {SessionUserId}",
                    request.UserId, userId);

                this.ToastError("Invalid verification session. Please sign in again.");
                return RedirectToAction(nameof(SignIn));
            }

            // For TOTP providers, we don't need to check session provider since no code was "sent"
            if (RequiresCodeSending(request.Provider) && sessionProvider != request.Provider)
            {
                _logger.LogWarning("2FA provider mismatch for user: {UserId}", request.UserId);
                this.ToastError("Invalid verification session. Please sign in again.");
                return RedirectToAction(nameof(SignIn));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                this.ToastError("Two-factor authentication session expired.");
                return RedirectToAction(nameof(SignIn));
            }

            var response = await _serviceManager.AuthService.Verify2FACodeAsync(request);

            if (!response.Successful)
            {
                _logger.LogWarning("2FA verification failed for user: {UserId}. Reason: {Reason}", 
                    request.UserId, response.Message);
                    
                ModelState.AddModelError(string.Empty, response.Message ?? "Verification failed. Please try again.");

                if (response.Message?.Contains("expired", StringComparison.OrdinalIgnoreCase) == true)
                {
                    this.ToastError(response.Message ?? "Verification code has expired. Please request a new one.");
                    return RedirectToAction(nameof(Send2FACode), new { returnUrl = request.ReturnUrl });
                }

                this.ToastError(response.Message ?? "An error occurred while verifying your code. Please try again later or contact IT support.");

                return RedirectToAction("TwoFactorLogin", new { request.ReturnUrl });
            }

            Clear2FASessionData();

            await SetAuthenticationTokenAsync(
                response.Data?.Token ?? string.Empty,
                response.Data?.RefreshToken ?? string.Empty,
                response.Data?.UserClaims.ToClaims());

            _logger.LogInformation("2FA verification successful for user: {UserId} from IP: {IpAddress}",
               request.UserId, HttpContext.Connection.RemoteIpAddress);

            this.ToastAuthSuccess("Two-factor authentication completed successfully!");

            if (response.Data?.UserInfo != null)
            {
                HttpContext.Session.SetString(SessionKey_UserProfile, JsonSerializer.Serialize(response.Data.UserInfo));
            }

            return RedirectToSafeUrl(request.ReturnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA code for user {UserId}", request.UserId);
            ModelState.AddModelError(string.Empty, "Verification failed. Please try again.");
            this.ToastError("An error occurred while verifying your code. Please try again later or contact IT support.");
            return View(request);
        }
    }

    #endregion

    #region Email Confirmation

    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Email confirmation attempted with invalid parameters from IP: {IpAddress}",
                HttpContext.Connection.RemoteIpAddress);

            this.ToastError("Invalid email confirmation link.");
            return RedirectToAction(nameof(SignIn));
        }

        try
        {
            var request = new ConfirmUserEmailRequest(email, token);
            var response = await _serviceManager.AuthService.ConfirmUserEmailAsync(request);

            if (response.Successful)
            {
                _logger.LogInformation("Email confirmed successfully for: {Email}", email);
                this.ToastAuthSuccess("Your email has been confirmed successfully! You can now sign in.");
            }
            else
            {
                _logger.LogWarning("Email confirmation failed for: {Email}. Reason: {Reason}", email, response.Message);
                this.ToastError(response.Message ?? "Email confirmation failed. Please try again.");
            }

            return RedirectToAction(nameof(SignIn));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for: {Email}", email);
            this.ToastError("Email confirmation failed.");
            return RedirectToAction(nameof(SignIn));
        }
    }

    [AllowAnonymous]
    public IActionResult ResendEmailConfirmation()
    {
        return View(new SendEmailConfirmationRequest(string.Empty));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmailConfirmation(SendEmailConfirmationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            var response = await _serviceManager.AuthService.ResendEmailConfirmationAsync(request);

            _logger.LogInformation("Email confirmation resend requested for: {Email}", request.Email);

            this.ToastActivitySuccess("email_confirmation_sent", "A new confirmation email has been sent. Please check your inbox.");

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to resend email confirmation for: {Email}. Reason: {Reason}", request.Email, response.Message);
                    
                ModelState.AddModelError(string.Empty, response.Message ?? "Failed to resend confirmation email. Please try again.");

                ViewBag.ErrorMessage = response.Message ?? "Failed to resend confirmation email. Please try again.";

                this.ToastError(response.Message ?? "Failed to resend confirmation email. Please try again.");

                return View(request);
            }

            return RedirectToAction(nameof(SignIn));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email confirmation for: {Email}", request.Email);
            this.ToastError("Error during sending confirmation email.");
            return View(request);
        }
    }

    #endregion

    #region Password Reset

    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        if (IsUserAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        } 

        return View(new ForgotPasswordRequest(string.Empty, string.Empty));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var validator = new ForgotPasswordRequestValidator();
        if (!await ValidateRequestAsync(request, validator, ModelState))
        {
            return View(request);
        }

        try
        {
            var (logoBase64, _, _) = await _serviceManager.FileService.ReadLogoAsync();

            var requestWithLogo = request with { LogoBase64 = logoBase64 };

            var response = await _serviceManager.AuthService.RequestPasswordResetAsync(requestWithLogo);

            if (!response.Successful)
            {
                _logger.LogWarning("Password reset request failed for email: {Email}. Reason: {Reason}",
                    request.Email, response.Message);

                // Add ModelState error for password reset failure
                ModelState.AddModelError(string.Empty, response.Message ?? "Password reset request failed. Please try again.");

                // Use ViewBag for error messages since we're returning the view directly
                ViewBag.ErrorMessage = response.Message ?? "Password reset request failed. Please try again.";


                return View(request);
            }

            this.ToastActivitySuccess("password_reset_requested", "A password reset link has been sent to your email. Please check your inbox.");

            _logger.LogInformation("Password reset requested for email: {Email} from IP: {IpAddress}",
                request.Email, HttpContext.Connection.RemoteIpAddress);
            
            this.ToastActivitySuccess("password_reset_requested", "A password reset link has been sent to your email. Please check your inbox.");

            return RedirectToAction(nameof(SignIn));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request for email: {Email}", request.Email);

            this.ToastError("An error occurred while processing your password reset request. Please try again later or contact IT support.");
            return View(request);
        }
    }

    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string token, string email)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Password reset attempted with invalid parameters from IP: {IpAddress}",
                HttpContext.Connection.RemoteIpAddress);

            this.ToastError("Invalid password reset link.");
            return RedirectToAction(nameof(SignIn));
        }

        try
        {
            var validationRequest = new ValidateResetTokenRequest(email, token);
            var response = await _serviceManager.AuthService.ValidatePasswordResetTokenAsync(validationRequest);

            if (!response.Successful)
            {
                _logger.LogWarning("Password reset token validation failed for email: {Email}. Reason: {Reason}", email, response.Message);

                this.ToastError(response.Message ?? "Invalid password reset link. Please request a new one.");

                return RedirectToAction(nameof(ForgotPassword));
            }

            var (logoBase64, _, _) = await _serviceManager.FileService.ReadLogoAsync();

            var resetRequest = new ResetPasswordRequest
            {
                Email = email,
                NewPassword = string.Empty,
                ConfirmPassword = string.Empty,
                Token = token,
                LogoBase64 = logoBase64

            };

            return View(resetRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password reset token for email: {Email}", email);
            this.ToastError("An error occurred while validating the password reset link. Please try again later or contact IT support.");
            return RedirectToAction(nameof(SignIn));
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var validator = new ResetPasswordRequestValidator();
        if (!await ValidateRequestAsync(request, validator, ModelState))
        {
            return View(request);
        }

        try
        {
            if (string.IsNullOrWhiteSpace(request.LogoBase64))
            {
                var (logoBase64, _, _) = await _serviceManager.FileService.ReadLogoAsync();
                request.LogoBase64 = logoBase64;
            }

            var response = await _serviceManager.AuthService.ResetPasswordAsync(request);

            if (!response.Successful)
            {
                _logger.LogWarning("Password reset failed for email: {Email}. Reason: {Reason}",
                    request.Email, response.Message);

                // Handle specific error cases
                if (response.Message?.Contains("expired", StringComparison.OrdinalIgnoreCase) == true ||
                    response.Message?.Contains("invalid", StringComparison.OrdinalIgnoreCase) == true)
                {
                    this.ToastError("This password reset link has expired or is invalid. Please request a new password reset.");
                    return RedirectToAction(nameof(ForgotPassword));
                }

                ModelState.AddModelError(string.Empty, response.Message ?? "Password reset failed. Please try again.");
                return View(request);
            }

            _logger.LogInformation("Password reset successful for email: {Email} from IP: {IpAddress}",
                request.Email, HttpContext.Connection.RemoteIpAddress);

            this.ToastAuthSuccess("Your password has been reset successfully! You can now sign in with your new password.");

            return RedirectToAction(nameof(SignIn));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for email: {Email}", request.Email);
            ModelState.AddModelError(string.Empty, "We're experiencing technical difficulties. Please try again later.");
            this.ToastError("An error occurred while resetting your password. Please try again later or contact IT support.");
            return View(request);
        }
    }


    #endregion

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SignOut(bool sessionExpired = false, string reason = "logout")
    {
        try
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sessionId = HttpContext.Session.GetString("SessionId") ?? Request.Cookies["session_id"];

            _logger.LogInformation("Sign out initiated. UserId: {UserId}, SessionId: {SessionId}, Reason: {Reason}", userId, sessionId, reason);
           
            // Call API to invalidate server-side tokens
            if (!string.IsNullOrWhiteSpace(userId))
            {
                try
                {
                    var response = await _serviceManager.AuthService.SignOutAsync();
                    if (!response.Successful)
                    {
                        _logger.LogWarning("API sign out failed for user: {UserId}. Message: {Message}",
                            userId, response.Message);
                    }
                    else
                    {
                        _logger.LogInformation("API sign out successful for user: {UserId}", userId);
                    }
                }
                catch (Exception apiEx)
                {
                    _logger.LogError(apiEx, "Error calling API sign out for user: {UserId}", userId);

                    this.ToastError("An error occurred while signing out. Please try again.");

                }

            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            ClearAllCookies(HttpContext);
            HttpContext.Session.Clear();

            if (sessionExpired)
            {
                _logger.LogInformation("User session expired: {UserId}", userId);
                this.ToastInfo("Your session has expired due to inactivity. Please sign in again.");
            }
            else
            {
                this.ToastAuthSuccess("You have successfully signed out.");
            }

            return RedirectToAction("SignIn", new { sessionExpired });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign out.");

            // Force cleanup even if there's an error
            try
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                ClearAllCookies(HttpContext);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Error during emergency cleanup");
            }

            this.ToastError("An error occurred while signing out.");

            return RedirectToAction("SignIn", new { sessionExpired = true });
        }
    }

    public async Task<IActionResult> SessionExpired(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();

        var cookieOptions = new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        };

        Response.Cookies.Delete("auth_token", cookieOptions);
        Response.Cookies.Delete("refresh_token", cookieOptions);

        _logger.LogInformation("Session expired redirect for user");

        return RedirectToAction("SignIn", new
        {
            returnUrl,
            sessionExpired = true
        });
    }

    #region Private Helper Methods

    private string GetProviderDisplayName(string provider)
    {
        return provider switch
        {
            TwoFactorConstants.Providers.Email => "Email",
            TwoFactorConstants.Providers.SMS => "Text Message",
            TwoFactorConstants.Providers.Authenticator => "Authenticator App",
            TwoFactorConstants.Providers.Phone => "Phone Call",
            _ => "Authentication"
        };
    }

    private string GenerateDeviceFingerprint()
    {
        var userAgent = Request.Headers["User-Agent"].ToString();
        var acceptLanguage = Request.Headers["Accept-Language"].ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var fingerprint = $"{userAgent}|{acceptLanguage}|{ipAddress}";

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(fingerprint));
        return Convert.ToBase64String(hash)[..16]; // First 16 characters
    }

    private static string ValidateHeader(string headerValue)
    {
        // Basic validation to prevent issues with malformed headers
        if (string.IsNullOrWhiteSpace(headerValue) || headerValue.Length > 1000)
        {
            return "unknown";
        }

        // Remove any control characters that could cause issues
        return new string(headerValue.Where(c => !char.IsControl(c)).ToArray());
    }

    private IActionResult? HandleLoginError(AppResponse<LoginResponse> response, LoginRequest request)
    {
        var message = response.Message?.ToLowerInvariant() ?? string.Empty;

        // Email confirmation required
        if (message.Contains("email") && message.Contains("confirm"))
        {

            _logger.LogInformation("Email confirmation required for user: {EmployeeNumber}", request.EmployeeNumber);
            this.ToastError("Please confirm your email address before signing in.");
            return RedirectToAction(nameof(ResendEmailConfirmation));
            
        }

        // Account locked
        if (message.Contains("locked"))
        {
            _logger.LogWarning("Account locked for user: {EmployeeNumber}", request.EmployeeNumber);
            this.ToastError("Your account has been locked. Please contact support.");
            
            return null;
        }

        return null; // No specific handling needed
    }

    private async Task<IActionResult> Handle2FARequirement(LoginResponse loginData, string? returnUrl)
    {
        HttpContext.Session.SetString(SessionKey_2FA_UserId, loginData.UserId);
        await HttpContext.Session.CommitAsync();

        _logger.LogInformation("2FA required for user: {UserId} from IP: {IpAddress}",
            loginData.UserId, HttpContext.Connection.RemoteIpAddress);

        TempData["UserId"] = loginData.UserId;
        TempData["ReturnUrl"] = returnUrl;

        this.ToastInfo("Two-factor authentication is required to complete your login.");

        return RedirectToAction(nameof(TwoFactorLogin), new { returnUrl });
    }

    private async Task SetAuthenticationTokenAsync(string token, string refreshToken, List<Claim>? claims = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        var tokenHandler = new JwtSecurityTokenHandler();
        DateTimeOffset tokenExpiry = DateTimeOffset.UtcNow.AddMinutes(60);

        if (!tokenHandler.CanReadToken(token))
        {
            _logger.LogError("❌ Cannot read JWT token");
            throw new ArgumentException("Invalid JWT token");
        }

        var jwtToken = tokenHandler.ReadJwtToken(token);
        if (jwtToken.ValidTo > DateTime.UtcNow)
        {
            tokenExpiry = jwtToken.ValidTo;
        }

        // Build claims identity
        var cookieClaims = new List<Claim>();

        // Add essential claims
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "nameid");

        if (userIdClaim != null)
            cookieClaims.Add(new Claim(ClaimTypes.NameIdentifier, userIdClaim.Value));

        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        if (emailClaim != null)
            cookieClaims.Add(new Claim(ClaimTypes.Email, emailClaim.Value));

        var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        if (nameClaim != null)
            cookieClaims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));

        var employeeNumberClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "EmployeeNumber");
        if (employeeNumberClaim != null)
            cookieClaims.Add(new Claim("EmployeeNumber", employeeNumberClaim.Value));

        // Add any additional claims from parameter
        if (claims != null && claims.Any())
            cookieClaims.AddRange(claims);

        var claimsIdentity = new ClaimsIdentity(
            cookieClaims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role
        );

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = tokenExpiry,
            AllowRefresh = true,
            IssuedUtc = DateTimeOffset.UtcNow,
            RedirectUri = null
        };

        try
        {
            // Sign in - this should create the authentication cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            var accessTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = tokenExpiry,
                Path = "/"
            };

            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8),
                Path = "/"
            };

            Response.Cookies.Append("auth_token", token, accessTokenCookieOptions);

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                Response.Cookies.Append("refresh_token", refreshToken, refreshTokenCookieOptions);
            }

            _logger.LogInformation("✅ SignInAsync completed - auth cookie will be sent to browser");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ SignInAsync failed");
            throw;
        }


        
    }

    private async Task<IActionResult> ReloadSend2FACodeView(Send2FACodeRequest request)
    {
        try
        {
            var providersResponse = await _serviceManager.AuthService.Get2FAProvidersAsync(new Get2FAProviderRequest(UserId: request.UserId));

            if (providersResponse.Successful && providersResponse.Data?.Providers != null)
            {
                request = request with
                {
                    Providers = providersResponse.Data.Providers.Select(p =>
                        new SelectListItem { Text = p.Text, Value = p.Value }).ToList()
                };
            }

            return View(request);
        }
        catch
        {
            return View(request);
        }
    }

    private void Clear2FASessionData()
    {
        string[] keysToRemove =
        [
            SessionKey_2FA_UserId,
            SessionKey_2FA_Provider,
            SessionKey_2FA_SentAt,
            SessionKey_2FA_ExpiresAt
        ];

        foreach (var key in keysToRemove)
        {
            HttpContext.Session.Remove(key);
            
        }
    }

    private static bool RequiresCodeSending(string? provider)
    {
        return provider switch
        {
            "Email" => true,
            "Phone" => true,
            "SMS" => true,
            "Authenticator" => false,
            "MicrosoftAuthenticator" => false,
            "TOTP" => false,
            _ => false
        };
    }

    private static bool IsTotpProvider(string? provider)
    {
        return provider?.ToLowerInvariant() switch
        {
            "authenticator" => true,
            "microsoftauthenticator" => true,
            "totp" => true,
            _ => false
        };
    }



    #endregion



}
