//using Azure;


using Azure;
using Azure.Core;

using EssPortal.Web.Blazor.Dtos.Auth;
using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using ESSPortal.Web.Blazor.Dtos.Auth;
using ESSPortal.Web.Blazor.Utilities.Api;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Options;

using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;

using ForgotPasswordRequest = ESSPortal.Application.Dtos.Auth.ForgotPasswordRequest;
using LoginRequest = ESSPortal.Application.Dtos.Auth.LoginRequest;
using ResetPasswordRequest = ESSPortal.Application.Dtos.Auth.ResetPasswordRequest;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.AppServices;

internal sealed class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceManager _serviceManager;
    private readonly IAppStateService _stateService;
    private readonly ILogger<AuthService> _logger;
    private const string SessionIdCookieName = "session_id";


    public AuthService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger, 
        IServiceManager serviceManager,
        IAppStateService stateService)
    {
        
        _httpContextAccessor = httpContextAccessor;
        _serviceManager = serviceManager;
        _logger = logger;
        _stateService = stateService;
    }


    public async Task<ApiResponse<bool>> RegisterEmployeeAsync(RegisterEmployeeRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.RegisterEmployeeAsync(request);

            return !apiResponse.Successful
                ? ApiResponse<bool>.Failure(apiResponse.Message!)
                : ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during employee registration");
            throw;
        }
    }
    
    public async Task<ApiResponse<bool>> SendEmailConfirmationAsync(SendEmailConfirmationRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.SendEmailConfirmationAsync(request);

            return !apiResponse.Successful
                ? ApiResponse<bool>.Failure(apiResponse.Message!)
                : ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email confirmation");
            throw;
        }
    }

    public async Task<ApiResponse<bool>> ResendEmailConfirmationAsync(SendEmailConfirmationRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.ResendEmailConfirmationAsync(request);

            return !apiResponse.Successful
                ? ApiResponse<bool>.Failure(apiResponse.Message!)
                : ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email confirmation");
            throw;
        }
    }

    public async Task<ApiResponse<bool>> ConfirmUserEmailAsync(ConfirmUserEmailRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.ConfirmUserEmailAsync(request);

            return !apiResponse.Successful
                ? ApiResponse<bool>.Failure(apiResponse.Message!)
                : ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming user email");
            throw;
        }
    }

    public async Task<ApiResponse<LoginResponse>> SignInAsync(LoginRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.SignInAsync(request);
            if (!apiResponse.Successful)
            {
                _logger.LogWarning("Login failed for {EmployeeNo}: {Message}", request.EmployeeNumber, apiResponse.Message);
                return ApiResponse<LoginResponse>.Failure(apiResponse.Message ?? "Unknown error during signing in.");
            }

            // Store authentication tokens securely
            await StoreAuthenticationTokensAsync(apiResponse.Data!);

            if (apiResponse.Data != null && !string.IsNullOrEmpty(apiResponse.SessionId))
            {
                await StoreSessionId(apiResponse.SessionId);
            }

            if (apiResponse.Data?.Requires2FA == true)
            {
                // If 2FA is required, return a specific response
                return ApiResponse<LoginResponse>.Success("2FA required", apiResponse.Data!);
            }

            _stateService.ClearCache();

            return !apiResponse.Successful
                ? ApiResponse<LoginResponse>.Failure(apiResponse.Message!)
                : ApiResponse<LoginResponse>.Success(apiResponse.Message!, apiResponse.Data!);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for employee: {EmployeeNumber}", request.EmployeeNumber);
            throw;
        }
    }

    public async Task<ApiResponse<SessionStatusResponse>> KeepAliveAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                var currentUser = await _stateService.LoadCurrentUserAsync(false);
                userId = currentUser?.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("Unlock failed: {Message}", "User not authenticated");
                    return ApiResponse<SessionStatusResponse>.Failure("User not authenticated");
                }
            }

            var sessionId = GetSessionId();

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Keep-alive failed: {Message}", "User not authenticated");
                return ApiResponse<SessionStatusResponse>.Failure("Keep-alive failed: User not authenticated");
            }

            // Update database session if session ID provided
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                var validationResult = await _serviceManager.SessionManagementService.IsSessionValidAsync(sessionId, userId);

                if (!validationResult.Successful)
                {
                    _logger.LogWarning("Session validation failed for user {UserId}, session {SessionId}: {Message}", userId, sessionId, validationResult.Message);
                        
                    return ApiResponse<SessionStatusResponse>.Failure(validationResult.Message ?? "Session is no longer valid");
                        
                }
            }

            return ApiResponse<SessionStatusResponse>.Success("Session extended", new SessionStatusResponse
            {
                IsValid = true,
                UserId = userId,
                SessionId = sessionId,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during keep-alive");
            throw;
        }
    }

    public async Task<ApiResponse<UnlockResponse>> UnlockSessionAsync(UnlockRequest unlockRequest)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                var currentUser = await _stateService.LoadCurrentUserAsync(false);
                userId = currentUser?.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("Unlock failed: {Message}", "User not authenticated");
                    return ApiResponse<UnlockResponse>.Failure("User not authenticated");
                }
            }

            var sessionId = GetSessionId();

            var verifyRequest = new VerifyPasswordRequest(
                userId,
                unlockRequest.Email ?? string.Empty,
                unlockRequest.EmployeeNumber ?? string.Empty,
                unlockRequest.Password);

            var response = await _serviceManager.AuthService.VerifyPasswordAsync(verifyRequest);

            if (!response.Successful)
            {
                _logger.LogWarning("Unlock failed for user {UserId}: {Message}", userId, response.Message);

                var isLocked = response.Message?.Contains("locked", StringComparison.OrdinalIgnoreCase) ?? false;

                return ApiResponse<UnlockResponse>.Failure(isLocked
                    ? "Account locked due to too many failed attempts" :
                    "Invalid password",
                    new UnlockResponse
                    {
                        Success = false,
                        AccountLocked = isLocked
                    });

            }

            // Update database session
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                var validationResult = await _serviceManager.SessionManagementService.IsSessionValidAsync(sessionId, userId);

                if (!validationResult.Successful)
                {
                    _logger.LogWarning("Session no longer valid during unlock for user {UserId}", userId);
                    return ApiResponse<UnlockResponse>.Failure("Session expired. Please sign in again.", new UnlockResponse
                    {
                        Success = false,
                        SessionExpired = true,
                        SessionId = sessionId ?? string.Empty

                    });

                }
            }

            return ApiResponse<UnlockResponse>.Success("Unlocked successfully", new UnlockResponse
            {
                Success = true,
                SessionId = sessionId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during unlock");
            throw;
        }
    }

    public async Task<ApiResponse<CurrentUserResponse>> GetCurrentUserAsync()
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.GetCurrentUserAsync();

            if (!apiResponse.Successful && IsTokenExpiredError(apiResponse!))
            {
                // Try to refresh token and retry
                var refreshResult = await RefreshTokenAsync();
                if (refreshResult.Successful)
                {
                    // Retry the original request with new token
                    apiResponse = await _serviceManager.AuthService.GetCurrentUserAsync();
                }
            }

            return !apiResponse.Successful
                ? ApiResponse<CurrentUserResponse>.Failure(apiResponse.Message!)
                : ApiResponse<CurrentUserResponse>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            throw;
        }
    }
    
    public async Task<ApiResponse<ProviderResponse>> Get2FAProvidersAsync(Get2FAProviderRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.Get2FAProvidersAsync(request);

            return !apiResponse.Successful
               ? ApiResponse<ProviderResponse>.Failure(apiResponse.Message!)
               : ApiResponse<ProviderResponse>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving 2FA providers for user: {UserId}", request.UserId);
            throw;
        }
    }
    
    public async Task<ApiResponse<Send2FACodeResponse>> Send2FACodeAsync(Send2FACodeRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.Send2FACodeAsync(request);

            return !apiResponse.Successful
               ? ApiResponse<Send2FACodeResponse>.Failure(apiResponse.Message!)
               : ApiResponse<Send2FACodeResponse>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending 2FA code for user: {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<ApiResponse<Verify2FACodeResponse>> Verify2FACodeAsync(Verify2FACodeRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.Verify2FACodeAsync(request);

            if (!apiResponse.Successful)
            {
                _logger.LogWarning("2FA verification failed for user {UserId}: {Message}", request.UserId, apiResponse.Message);
                return ApiResponse<Verify2FACodeResponse>.Failure(apiResponse.Message ?? "Unknown error during 2FA verification");
            }

            if (apiResponse.Successful && apiResponse.Data != null)
            {
                await Store2FAAuthenticationTokensAsync(apiResponse.Data);

                // Store session ID from response header
                if (!string.IsNullOrWhiteSpace(apiResponse.SessionId))
                {
                    await StoreSessionId(apiResponse.SessionId);
                }
            }

            return !apiResponse.Successful
                   ? ApiResponse<Verify2FACodeResponse>.Failure(apiResponse.Message!)
                   : ApiResponse<Verify2FACodeResponse>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA code for user: {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.RequestPasswordResetAsync(request);
            if (!apiResponse.Successful) 
            {
                _logger.LogWarning("Password reset request failed for email {Email}: {Message}", request.Email, apiResponse.Message);
                return ApiResponse<bool>.Failure(apiResponse.Message ?? "Unknown error during password reset request.");
            }

            return !apiResponse.Successful
                   ? ApiResponse<bool>.Failure(apiResponse.Message!)
                   : ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset");
            throw;
        }
    }
    
    public async Task<ApiResponse<bool>> ValidatePasswordResetTokenAsync(ValidateResetTokenRequest validateResetTokenRequest)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.ValidatePasswordResetTokenAsync(validateResetTokenRequest);
            if (!apiResponse.Successful)
            {
                _logger.LogWarning("Password reset token validation failed for email {Email}: {Message}", validateResetTokenRequest.Email, apiResponse.Message);
                return ApiResponse<bool>.Failure(apiResponse.Message ?? "Unknown error during password reset token validation.");
            }

            return !apiResponse.Successful
               ? ApiResponse<bool>.Failure(apiResponse.Message!)
               : ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password reset token");
            throw;
        }
    }
    
    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.ResetPasswordAsync(request);
            if (!apiResponse.Successful) 
            {
                _logger.LogWarning("Password reset failed for email {Email}: {Message}", request.Email, apiResponse.Message);
                return ApiResponse<bool>.Failure(apiResponse.Message ?? "Unknown error during password reset.");
            }

            return !apiResponse.Successful
                   ? ApiResponse<bool>.Failure(apiResponse.Message!)
                   : ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            throw;
        }
    }

    public async Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync()
    {
        try
        {
            var currentTokens = GetStoredTokens();
            if (currentTokens == null)
            {
                _logger.LogWarning("No authentication tokens found for refresh");
                return ApiResponse<RefreshTokenResponse>.Failure("No authentication tokens found. Please sign in again.");
            }

            var (accessToken, refreshToken) = currentTokens.Value;

            var refreshRequest = new RefreshTokenRequest(accessToken, refreshToken);

            var apiResponse = await _serviceManager.AuthService.RefreshTokenAsync(refreshRequest);

            if (!apiResponse.Successful)
            {
                _logger.LogWarning("Token refresh failed: {Message}", apiResponse.Message);
                ClearStoredTokens();
                return ConvertApiResponse(apiResponse!);
            }

            // Store new tokens
            await StoreRefreshTokensAsync(apiResponse.Data!);

            return !apiResponse.Successful
                   ? ApiResponse<RefreshTokenResponse>.Failure(apiResponse.Message!)
                   : ApiResponse<RefreshTokenResponse>.Success(apiResponse.Message!, apiResponse.Data!);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            ClearStoredTokens();
            throw;
        }
    }

    public async Task<ApiResponse<bool>> SignOutAsync()
    {
        try
        {
            var tokens = GetStoredTokens();
            if (tokens == null || string.IsNullOrWhiteSpace(tokens.Value.AccessToken))
            {
                _logger.LogInformation("No valid token found, performing local sign out only");
                ClearStoredTokens();
                ClearSessionId();
                return ApiResponse<bool>.Success("Signed out successfully.", true);
            }

            var apiResponse = await _serviceManager.AuthService.SignOutAsync();

            // Always clear local tokens regardless of API response
            ClearStoredTokens();
            ClearSessionId();

            if (!apiResponse.Successful)
            {
                _logger.LogWarning("API sign out failed: {Message}", apiResponse.Message);

                // Still return success since local cleanup is done
                return ApiResponse<bool>.Success("Signed out locally.", true);
            }

            return ApiResponse<bool>.Success(apiResponse.Message ?? "Signed out successfully.", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign out");
            // Clear tokens even if API call fails
            ClearStoredTokens();
            ClearSessionId();
            throw;
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

    public async Task<ApiResponse<bool>> VerifyPasswordAsync(VerifyPasswordRequest verifyPasswordRequest)
    {
        try
        {
            var apiResponse = await _serviceManager.AuthService.VerifyPasswordAsync(verifyPasswordRequest);

            return !apiResponse.Successful
                   ? ApiResponse<bool>.Failure(apiResponse.Message!)
                   : ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data!);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            throw;
        }
    }

    public string? GetSessionId()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[SessionIdCookieName];
    }

    #endregion

    #region Private Helper Methods

    private ApiResponse<T> ConvertApiResponse<T>(ApiResponse<T?> apiResponse)
    {
        if (apiResponse.Successful && apiResponse.Data != null)
        {
            return ApiResponse<T>.Success(apiResponse.Message!, apiResponse.Data);
        }

        return ApiResponse<T>.Failure(apiResponse.Message ?? "Unknown error");

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

        httpContext.Response.Cookies.Append("auth_token", refreshResponse.Token, accessTokenCookieOptions);
        httpContext.Response.Cookies.Append("refresh_token", refreshResponse.RefreshToken, refreshTokenCookieOptions);

        // Update user profile if provided
        if (refreshResponse.UserInfo != null)
        {
            httpContext.Session.SetString("UserProfile", JsonSerializer.Serialize(refreshResponse.UserInfo));
        }

        await httpContext.Session.CommitAsync();
    }

    private async Task StoreSessionInfoAsync<T>(ApiResponse<T> response) where T : class
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

    private bool IsTokenExpiredError(ApiResponse<CurrentUserResponse?> response)
    {
        return !response.Successful &&
               (response.Message?.Contains("expired", StringComparison.OrdinalIgnoreCase) == true ||
                response.Message?.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) == true);
    }

    private async Task StoreSessionId(string sessionId)
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
        await httpContext.Session.CommitAsync();
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
