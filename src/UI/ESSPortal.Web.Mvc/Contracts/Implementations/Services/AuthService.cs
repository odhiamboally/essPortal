//using Azure;


using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Auth;
using EssPortal.Web.Mvc.Dtos.Common;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Dtos.Auth;
using ESSPortal.Web.Mvc.Utilities.Api;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;

using ForgotPasswordRequest = EssPortal.Web.Mvc.Dtos.Auth.ForgotPasswordRequest;
using LoginRequest = EssPortal.Web.Mvc.Dtos.Auth.LoginRequest;
using RegisterEmployeeRequest = EssPortal.Web.Mvc.Dtos.Auth.RegisterEmployeeRequest;
using ResetPasswordRequest = EssPortal.Web.Mvc.Dtos.Auth.ResetPasswordRequest;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.AppServices;

internal sealed class AuthService : IAuthService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string SessionIdCookieName = "session_id";


    public AuthService(IApiService apiService, IOptions<ApiSettings> apiSettings,  ILogger<AuthService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }


    public async Task<AppResponse<bool>> RegisterEmployeeAsync(RegisterEmployeeRequest registerRequest)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.RegisterEmployee;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.PostAsync<RegisterEmployeeRequest, bool>(endpoint, registerRequest);

            return !apiResponse.Successful
                ? AppResponse<bool>.Failure(apiResponse.Message!)
                : AppResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during employee registration");
            return AppResponse<bool>.Failure("Registration failed. Please try again.");
        }
    }
    
    public async Task<AppResponse<bool>> SendEmailConfirmationAsync(SendEmailConfirmationRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.SendEmailConfirmation;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.PostAsync<SendEmailConfirmationRequest, bool>(endpoint, request);

            return !apiResponse.Successful
                ? AppResponse<bool>.Failure(apiResponse.Message!)
                : AppResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email confirmation");
            return AppResponse<bool>.Failure("Failed to send confirmation email. Please try again.");
        }
    }

    public async Task<AppResponse<bool>> ResendEmailConfirmationAsync(SendEmailConfirmationRequest request)
    {
        try
        {

            var endpoint = _apiSettings.ApiEndpoints?.Auth?.ResendEmailConfirmation;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.PostAsync<SendEmailConfirmationRequest, bool>(endpoint, request);

            return !apiResponse.Successful
                ? AppResponse<bool>.Failure(apiResponse.Message!)
                : AppResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email confirmation");
            return AppResponse<bool>.Failure("Failed to resend confirmation email. Please try again.");
        }
    }

    public async Task<AppResponse<bool>> ConfirmUserEmailAsync(ConfirmUserEmailRequest confirmEmailRequest)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.ConfirmUserEmail;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.PostAsync<ConfirmUserEmailRequest, bool>(endpoint, confirmEmailRequest);

            return !apiResponse.Successful
                ? AppResponse<bool>.Failure(apiResponse.Message!)
                : AppResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming user email");
            return AppResponse<bool>.Failure("Email confirmation failed. Please try again.");
        }
    }

    public async Task<AppResponse<LoginResponse>> SignInAsync(LoginRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.Login;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.PostAsync<LoginRequest, LoginResponse>(endpoint, request);
            if (!apiResponse.Successful)
            {
                return ConvertApiResponse(apiResponse);
            }

            // Store authentication tokens securely
            await StoreAuthenticationTokensAsync(apiResponse.Data!);

            if (!string.IsNullOrEmpty(apiResponse.SessionId))
            {
                StoreSessionId(apiResponse.SessionId);
                if (apiResponse.Data != null)
                {
                    await StoreSessionInfoAsync(AppResponse<LoginResponse>.Success(apiResponse.Message!, apiResponse.Data));
                }
            }

            if (apiResponse.Data?.Requires2FA == true)
            {
                // If 2FA is required, return a specific response
                return AppResponse<LoginResponse>.Success("2FA required", apiResponse.Data!);
            }

            return !apiResponse.Successful
                ? AppResponse<LoginResponse>.Failure(apiResponse.Message!)
                : AppResponse<LoginResponse>.Success(apiResponse.Message!, apiResponse.Data!);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for employee: {EmployeeNumber}", request.EmployeeNumber);
            return AppResponse<LoginResponse>.Failure("Login failed. Please try again.");
        }
    }

    public async Task<AppResponse<SessionStatusResponse>> KeepAliveAsync()
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.SessionKeepAlive;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            // Session ID is automatically included via SetAuthorizationHeader()
            var apiResponse = await _apiService.PostAsync<object, SessionStatusResponse>(endpoint, null);

            if (!apiResponse.Successful)
            {
                _logger.LogWarning("API Keep-alive failed: {Message}", apiResponse.Message);
                return AppResponse<SessionStatusResponse>.Failure(apiResponse.Message!);
            }

            return AppResponse<SessionStatusResponse>.Success("Session extended", apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during keep-alive");
            return AppResponse<SessionStatusResponse>.Failure("Keep-alive failed");
        }
    }

    public async Task<AppResponse<UnlockResponse>> UnlockSessionAsync(UnlockRequest unlockRequest)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.SessionUnlock;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.PostAsync<UnlockRequest, UnlockResponse>(endpoint, unlockRequest);

            if (!apiResponse.Successful)
            {
                return AppResponse<UnlockResponse>.Failure(apiResponse.Message!, apiResponse.Data);
            }

            return AppResponse<UnlockResponse>.Success("Unlocked successfully", apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during unlock");
            return AppResponse<UnlockResponse>.Failure("Unlock failed");
        }
    }

    public async Task<AppResponse<CurrentUserResponse>> GetCurrentUserAsync()
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.GetCurrentUser;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.GetAsync<CurrentUserResponse>(endpoint);

            if (!apiResponse.Successful && IsTokenExpiredError(apiResponse))
            {
                // Try to refresh token and retry
                var refreshResult = await RefreshTokenAsync();
                if (refreshResult.Successful)
                {
                    // Retry the original request with new token
                    apiResponse = await _apiService.GetAsync<CurrentUserResponse>(endpoint);
                }
            }

            return !apiResponse.Successful
                ? AppResponse<CurrentUserResponse>.Failure(apiResponse.Message!)
                : AppResponse<CurrentUserResponse>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return AppResponse<CurrentUserResponse>.Failure("Failed to retrieve user information.");
        }
    }
    
    public async Task<AppResponse<ProviderResponse>> Get2FAProvidersAsync(Get2FAProviderRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.Get2FAProviders;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
            endpoint = EndpointHelper.ReplaceParams(endpoint, new Dictionary<string, string>
            {
                { "userId", request.UserId }
            });

            var apiResponse = await _apiService.GetAsync<ProviderResponse>(endpoint);

            return !apiResponse.Successful
               ? AppResponse<ProviderResponse>.Failure(apiResponse.Message!)
               : AppResponse<ProviderResponse>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving 2FA providers for user: {UserId}", request.UserId);
            return AppResponse<ProviderResponse>.Failure("Failed to retrieve 2FA providers.");
        }
    }
    
    public async Task<AppResponse<Send2FACodeResponse>> Send2FACodeAsync(Send2FACodeRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.Send2FACode;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.PostAsync<Send2FACodeRequest, Send2FACodeResponse>(endpoint, request);

            return !apiResponse.Successful
               ? AppResponse<Send2FACodeResponse>.Failure(apiResponse.Message!)
               : AppResponse<Send2FACodeResponse>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending 2FA code for user: {UserId}", request.UserId);
            return AppResponse<Send2FACodeResponse>.Failure("Failed to send verification code.");
        }
    }

    public async Task<AppResponse<Verify2FACodeResponse>> Verify2FACodeAsync(Verify2FACodeRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.Verify2FACode;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.PostAsync<Verify2FACodeRequest, Verify2FACodeResponse>(endpoint, request);

            if (!apiResponse.Successful)
            {
                return ConvertApiResponse(apiResponse);
            }

            if (apiResponse.Successful && apiResponse.Data != null)
            {
                // Store authentication tokens after successful 2FA
                await Store2FAAuthenticationTokensAsync(apiResponse.Data);

                // Store session ID from response header
                if (apiResponse.Headers != null && apiResponse.Headers.TryGetValue("X-Session-Id", out var sessionId) &&
                    !string.IsNullOrEmpty(sessionId))
                {
                    StoreSessionId(sessionId);
                }
            }

            return !apiResponse.Successful
                   ? AppResponse<Verify2FACodeResponse>.Failure(apiResponse.Message!)
                   : AppResponse<Verify2FACodeResponse>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA code for user: {UserId}", request.UserId);
            return AppResponse<Verify2FACodeResponse>.Failure("Verification failed. Please try again.");
        }
    }


    public async Task<AppResponse<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.RequestPasswordReset;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.PostAsync<ForgotPasswordRequest, bool>(endpoint, request);

            return !apiResponse.Successful
                   ? AppResponse<bool>.Failure(apiResponse.Message!)
                   : AppResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset");
            return AppResponse<bool>.Failure("Password reset request failed.");
        }
    }
    
    public async Task<AppResponse<bool>> ValidatePasswordResetTokenAsync(ValidateResetTokenRequest validateResetTokenRequest)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.ValidatePasswordResetToken;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.PostAsync<ValidateResetTokenRequest, bool>(endpoint, validateResetTokenRequest);
            return !apiResponse.Successful
               ? AppResponse<bool>.Failure(apiResponse.Message!)
               : AppResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password reset token");
            return AppResponse<bool>.Failure("Token validation failed.");
        }
    }
    
    public async Task<AppResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.ResetPassword;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await _apiService.PostAsync<ResetPasswordRequest, bool>(endpoint, request);

            return !apiResponse.Successful
                   ? AppResponse<bool>.Failure(apiResponse.Message!)
                   : AppResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return AppResponse<bool>.Failure("Password reset failed.");
        }
    }


    public async Task<AppResponse<RefreshTokenResponse>> RefreshTokenAsync()
    {
        try
        {
            var currentTokens = GetStoredTokens();
            if (currentTokens == null)
            {
                _logger.LogWarning("No authentication tokens found for refresh");
                return AppResponse<RefreshTokenResponse>.Failure("No authentication tokens found. Please sign in again.");
            }

            var (accessToken, refreshToken) = currentTokens.Value;

            var endpoint = _apiSettings.ApiEndpoints?.Auth?.RefreshToken;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var refreshRequest = new RefreshTokenRequest(accessToken, refreshToken);

            var apiResponse = await _apiService.PostAsync<RefreshTokenRequest, RefreshTokenResponse>(endpoint, refreshRequest);

            if (!apiResponse.Successful)
            {
                _logger.LogWarning("Token refresh failed: {Message}", apiResponse.Message);
                // Clear stored tokens on refresh failure
                ClearStoredTokens();
                return ConvertApiResponse(apiResponse);
            }

            // Store new tokens
            await StoreRefreshTokensAsync(apiResponse.Data!);

            _logger.LogInformation("Token refreshed successfully for user");
            return ConvertApiResponse(apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            ClearStoredTokens();
            return AppResponse<RefreshTokenResponse>.Failure("Token refresh failed. Please sign in again.");
        }
    }

    public async Task<AppResponse<bool>> SignOutAsync()
    {
        try
        {
            
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.SignOut;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                ClearStoredTokens();
                ClearSessionId();
                return AppResponse<bool>.Failure("Endpoint is not configured.");
            }

            // Check if we have a valid token before making the API call
            var tokens = GetStoredTokens();
            if (tokens == null || string.IsNullOrWhiteSpace(tokens.Value.AccessToken))
            {
                _logger.LogInformation("No valid token found, performing local sign out only");
                ClearStoredTokens();
                ClearSessionId();
                return AppResponse<bool>.Success("Signed out successfully.", true);
            }

            var apiResponse = await _apiService.PostAsync<object, bool>(endpoint, null);

            // Always clear local tokens regardless of API response
            ClearStoredTokens();
            ClearSessionId();

            if (!apiResponse.Successful)
            {
                _logger.LogWarning("API sign out failed: {Message}", apiResponse.Message);

                // Still return success since local cleanup is done
                return AppResponse<bool>.Success("Signed out locally.", true);
            }

            return AppResponse<bool>.Success(apiResponse.Message ?? "Signed out successfully.", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign out");
            // Clear tokens even if API call fails
            ClearStoredTokens();
            ClearSessionId();
            return AppResponse<bool>.Success("Signed out successfully.", true);
        }

    }




    #region Authentication State Management

    public bool IsAuthenticated()
    {
        var tokens = GetStoredTokens();
        if (tokens == null)
        {
            return false;
        }
        
        var (accessToken, _) = tokens.Value;
        return tokens != null && !string.IsNullOrWhiteSpace(accessToken);
    }

    public string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.Session.GetString("UserId");
    }

    public async Task<bool> EnsureAuthenticatedAsync()
    {
        if (!IsAuthenticated())
        {
            return false;
        }

        // Check if current user call succeeds (validates token)
        var currentUserResponse = await GetCurrentUserAsync();
        return currentUserResponse.Successful;
    }

    public async Task<AppResponse<bool>> VerifyPasswordAsync(VerifyPasswordRequest verifyPasswordRequest)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints?.Auth?.VerifyPassword;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
            
            var apiResponse = await _apiService.PostAsync<VerifyPasswordRequest, bool>(endpoint, verifyPasswordRequest);

            return !apiResponse.Successful
                   ? AppResponse<bool>.Failure(apiResponse.Message!)
                   : AppResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);

        }
        catch (Exception)
        {

            throw;
        }
    }

    public string? GetSessionId()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[SessionIdCookieName];
    }

    #endregion

    #region Private Helper Methods

    private AppResponse<T> ConvertApiResponse<T>(AppResponse<T?> apiResponse)
    {
        if (apiResponse.Successful && apiResponse.Data != null)
        {
            return AppResponse<T>.Success(apiResponse.Message!, apiResponse.Data);
        }

        return AppResponse<T>.Failure(apiResponse.Message ?? "Unknown error");

    }

    private async Task StoreAuthenticationTokensAsync(LoginResponse loginResponse)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        // Store tokens in secure cookies
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = loginResponse.TokenExpiresAt
        };

        httpContext.Response.Cookies.Append("auth_token", loginResponse.Token, cookieOptions);

        if (!string.IsNullOrWhiteSpace(loginResponse.RefreshToken))
        {
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7) // Refresh tokens usually have longer expiry
            };
            httpContext.Response.Cookies.Append("refresh_token", loginResponse.RefreshToken, refreshCookieOptions);
        }

        // Store user info in session
        httpContext.Session.SetString("UserId", loginResponse.UserId);

        if (loginResponse.UserInfo != null)
        {
            httpContext.Session.SetString("UserProfile", JsonSerializer.Serialize(loginResponse.UserInfo));
        }

        await httpContext.Session.CommitAsync();
    }

    private async Task Store2FAAuthenticationTokensAsync(Verify2FACodeResponse verifyResponse)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = verifyResponse.ExpiresAt
        };

        httpContext.Response.Cookies.Append("auth_token", verifyResponse.Token, cookieOptions);

        if (!string.IsNullOrWhiteSpace(verifyResponse.RefreshToken))
        {
            httpContext.Response.Cookies.Append("refresh_token", verifyResponse.RefreshToken, cookieOptions);
        }

        httpContext.Session.SetString("UserId", verifyResponse.UserId);

        if (verifyResponse.UserInfo != null)
        {
            httpContext.Session.SetString("UserInfo", JsonSerializer.Serialize(verifyResponse.UserInfo));
        }

        await httpContext.Session.CommitAsync();
    }

    private async Task StoreRefreshTokensAsync(RefreshTokenResponse refreshResponse)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var accessTokenCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(60), // Match JWT access token (3 min)
            Path = "/"
        };

        var refreshTokenCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(8), // Match JWT refresh token (8 hours)
            Path = "/"
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("auth_token");
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("refresh_token");

        httpContext.Response.Cookies.Append("auth_token", refreshResponse.AccessToken, accessTokenCookieOptions);
        httpContext.Response.Cookies.Append("refresh_token", refreshResponse.RefreshToken, refreshTokenCookieOptions);

        // Update user profile if provided
        if (refreshResponse.UserInfo != null)
        {
            httpContext.Session.SetString("UserProfile", JsonSerializer.Serialize(refreshResponse.UserInfo));
        }

        await httpContext.Session.CommitAsync();
    }

    private async Task StoreSessionInfoAsync<T>(AppResponse<T> response) where T : class
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var sessionCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15), 
            Path = "/"
        };


        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("session_id");

        httpContext.Response.Cookies.Append("session_id", response.SessionId ?? string.Empty, sessionCookieOptions);

        await httpContext.Session.CommitAsync();
    }

    private (string AccessToken, string RefreshToken)? GetStoredTokens()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogWarning("HttpContext is null when trying to get stored tokens");
            return null;
        }

        var accessToken = httpContext.Request.Cookies["auth_token"];
        var refreshToken = httpContext.Request.Cookies["refresh_token"];

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            _logger.LogDebug("Access token not found in cookies");
            return null;
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogDebug("Refresh token not found in cookies");
            return null;
        }

        return (accessToken, refreshToken);
    }

    private void ClearStoredTokens()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        // Clear cookies with proper options
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        };

        httpContext.Response.Cookies.Delete("auth_token", cookieOptions);
        httpContext.Response.Cookies.Delete("refresh_token", cookieOptions);

        // Clear session
        httpContext.Session.Clear();
    }

    private bool IsTokenExpiredError(AppResponse<CurrentUserResponse?> response)
    {
        return !response.Successful &&
               (response.Message?.Contains("expired", StringComparison.OrdinalIgnoreCase) == true ||
                response.Message?.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) == true);
    }

    private void StoreSessionId(string sessionId)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null || string.IsNullOrEmpty(sessionId)) return;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(8), // Match refresh token lifetime
            Path = "/"
        };

        httpContext.Response.Cookies.Append(SessionIdCookieName, sessionId, cookieOptions);
        _logger.LogDebug("Session ID stored: {SessionId}", sessionId);
    }

    private void ClearSessionId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var cookieOptions = new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        };

        httpContext.Response.Cookies.Delete(SessionIdCookieName, cookieOptions);
    }



    #endregion



}
