using Asp.Versioning;

using ESSPortal.Application.Constants;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.TwoFactor;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using System.Security.Claims;


namespace ESSPortal.Api.Controllers;


[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public class AuthController : ControllerBase
{
    
    private readonly ILogger<AuthController> _logger; 
    private readonly IServiceManager _serviceManager;
    private readonly string ipAddress;
    private readonly string userAgent;

    public AuthController(
        ILogger<AuthController> logger,
        IConfiguration configuration,
        IServiceManager serviceManager
        )
    {

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        ipAddress = "unknown";
        userAgent = "unknown";

    }


   
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.Authentication)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<bool>>> Register([FromBody] RegisterEmployeeRequest request)
    {
        
        var response = await _serviceManager.AuthService.RegisterEmployeeAsync(request);

        if (!response.Successful)
        {
            // Check if it's a duplicate registration
            if (response.Message?.Contains("already registered", StringComparison.OrdinalIgnoreCase) == true ||
                response.Message?.Contains("already exists", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Conflict(response);
            }
            return BadRequest(response);
        }

        return StatusCode(StatusCodes.Status201Created, response); // return without location
        //return CreatedAtAction(nameof(GetCurrentUser), response);
    }

    
    [HttpPost("email/send-confirmation")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.PasswordReset)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SendEmailConfirmation([FromBody] SendEmailConfirmationRequest request)
    {
        var response = await _serviceManager.AuthService.SendEmailConfirmationAsync(request);
        // Always return 200 for security (don't reveal if email exists)
        return Ok(response);
    }

    
    [HttpPost("email/resend-confirmation")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.PasswordReset)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> ReSendEmailConfirmation([FromBody] SendEmailConfirmationRequest request)
    {
        var response = await _serviceManager.AuthService.ResendEmailConfirmationAsync(request);
        // Always return 200 for security (don't reveal if email exists)
        return Ok(response);
    }


    [HttpPost("email/confirm")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.PasswordReset)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ConfirmEmail([FromBody] ConfirmUserEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(ApiResponse<bool>.Failure("Email and token are required"));
        }

        var response = await _serviceManager.AuthService.ConfirmUserEmailAsync(request);

        if (!response.Successful)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

   
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.Login)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status202Accepted)] 
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status423Locked)] 
    public async Task<ActionResult<ApiResponse<LoginResponse>>> SignIn([FromBody] LoginRequest request)
    {
        
        _logger.LogInformation("Login attempt from IP: {IpAddress}, User Agent: {UserAgent}", ipAddress, userAgent);
        
        var response = await _serviceManager.AuthService.SignInAsync(request);

        if (!response.Successful)
        {
            // Handle specific login failure scenarios
            if (response.Message?.Contains("locked", StringComparison.OrdinalIgnoreCase) == true)
            {
                return StatusCode(StatusCodes.Status423Locked, response);
            }
            if (response.Message?.Contains("confirm", StringComparison.OrdinalIgnoreCase) == true)
            {
                return StatusCode(StatusCodes.Status403Forbidden, response);
            }
            return Unauthorized(response);
        }

        if (response.Data?.Requires2FA == true)
        {
            return Accepted(response); // 202 - Additional action required
        }

        // Check concurrent sessions and create new session
        if (response.Data?.UserId != null)
        {
            var sessionId = Guid.NewGuid().ToString();
            var createSessionResponse = await _serviceManager.SessionManagementService.CreateSessionAsync(
                response.Data.UserId, 
                sessionId, 
                ipAddress, 
                userAgent, 
                request.DeviceFingerprint);

            if (!createSessionResponse.Successful)
            {
                _logger.LogError("Failed to create session for user: {UserId}, Error: {Error}", response.Data.UserId, createSessionResponse.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<LoginResponse>.Failure(createSessionResponse.Message ?? "Failed to create session"));
            }

            Response.Headers["X-Session-Id"] = sessionId;

            response = response with { SessionId = sessionId};
        }

        return Ok(response);
    }

    [HttpPost("session/keep-alive")]
    [Authorize]
    [EnableRateLimiting(RateLimitingPolicies.Api)]
    [ProducesResponseType(typeof(ApiResponse<SessionStatusResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SessionStatusResponse>>> SessionKeepAlive()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<SessionStatusResponse>.Failure("User not authenticated"));
        }

        // Update database session if session ID provided
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var validationResult = await _serviceManager.SessionManagementService.IsSessionValidAsync(sessionId, userId);
                
            if (!validationResult.Successful)
            {
                _logger.LogWarning(
                    "Session validation failed for user {UserId}, session {SessionId}: {Message}",
                    userId, sessionId, validationResult.Message);

                // Session was ended elsewhere (admin, concurrent limit, etc.)
                return Unauthorized(ApiResponse<SessionStatusResponse>.Failure(
                    validationResult.Message ?? "Session is no longer valid"));
            }
        }

        _logger.LogDebug("Keep-alive successful for user {UserId}, session {SessionId}", userId, sessionId);

        return Ok(ApiResponse<SessionStatusResponse>.Success("Session extended", new SessionStatusResponse
        {
            IsValid = true,
            UserId = userId,
            SessionId = sessionId,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15) // Match your session timeout
        }));
    }

    [HttpPost("session/unlock")]
    [Authorize]
    [EnableRateLimiting(RateLimitingPolicies.Authentication)]
    [ProducesResponseType(typeof(ApiResponse<UnlockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UnlockResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UnlockResponse>>> SessionUnlock([FromBody] UnlockRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<UnlockResponse>.Failure("User not authenticated"));
        }

        if (string.IsNullOrWhiteSpace(request?.Password))
        {
            return BadRequest(ApiResponse<UnlockResponse>.Failure("Password is required"));
        }

        // Verify password
        var verifyRequest = new VerifyPasswordRequest(
            userId,
            request.Email ?? string.Empty,
            request.EmployeeNumber ?? string.Empty,
            request.Password);

        var response = await _serviceManager.AuthService.VerifyPasswordAsync(verifyRequest);

        if (!response.Successful)
        {
            _logger.LogWarning("Unlock failed for user {UserId}: {Message}", userId, response.Message);

            var isLocked = response.Message?.Contains("locked", StringComparison.OrdinalIgnoreCase) ?? false;

            return Unauthorized(ApiResponse<UnlockResponse>.Failure(
                isLocked ? "Account locked due to too many failed attempts" : "Invalid password",
                new UnlockResponse { Success = false, AccountLocked = isLocked }));
        }

        // Update database session
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var validationResult = await _serviceManager.SessionManagementService
                .IsSessionValidAsync(sessionId, userId);

            if (!validationResult.Successful)
            {
                _logger.LogWarning("Session no longer valid during unlock for user {UserId}", userId);
                return Unauthorized(ApiResponse<UnlockResponse>.Failure("Session expired. Please sign in again.", new UnlockResponse 
                { 
                    Success = false, 
                    SessionExpired = true, 
                    SessionId = sessionId ?? string.Empty

                }));
                    
            }
        }

        _logger.LogInformation("Unlock successful for user {UserId}", userId);

        return Ok(ApiResponse<UnlockResponse>.Success("Unlocked successfully", new UnlockResponse
        {
            Success = true,
            SessionId = sessionId
        }));
    }

    [HttpGet("me", Name ="GetCurrentUser")]
    [Authorize]
    [EnableRateLimiting(RateLimitingPolicies.Api)]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<CurrentUserResponse>>> GetCurrentUser()
    {
        var response = await _serviceManager.AuthService.GetCurrentUserAsync();

        if (!response.Successful)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    
    [HttpGet("2fa/providers/{userId}")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.Api)]
    [ProducesResponseType(typeof(ApiResponse<ProviderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProviderResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProviderResponse>>> Get2FAProviders([FromRoute] string userId)
    {
        var request = new Get2FAProviderRequest (userId);
        var response = await _serviceManager.AuthService.Get2FAProvidersAsync(request);

        if (!response.Successful)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

   
    [HttpPost("2fa/send-code")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.TwoFactor)]
    [ProducesResponseType(typeof(ApiResponse<Send2FACodeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Send2FACodeResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Send2FACodeResponse>>> Send2FACode([FromBody] Send2FACodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest(ApiResponse<Send2FACodeResponse>.Failure("User ID is required"));
        }

        var response = await _serviceManager.AuthService.Send2FACodeAsync(request);

        if (!response.Successful)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

   
    [HttpPost("2fa/verify")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.TwoFactor)]
    [ProducesResponseType(typeof(ApiResponse<Verify2FACodeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Verify2FACodeResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<Verify2FACodeResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<Verify2FACodeResponse>>> Verify2FACode([FromBody] Verify2FACodeRequest request)
    {
        
        

        var verifyTotpRequest = new VerifyTotpCodeRequest
        {
            UserId = request.UserId,
            Code = request.Code,
        };

        var response = await _serviceManager.TwoFactorService.VerifyTotpCodeAsync(verifyTotpRequest);

        if (!response.Successful)
        {
            if (response.Message?.Contains("Invalid", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Unauthorized(response);
            }
            return BadRequest(response);
        }

        // Create session after successful 2FA
        if (response.Data != null)
        {
            var sessionId = Guid.NewGuid().ToString();
            await _serviceManager.SessionManagementService.CreateSessionAsync(request.UserId, sessionId, ipAddress, userAgent, request.DeviceFingerprint ?? "unknown");

            Response.Headers["X-Session-Id"] = sessionId;

            response = response with { SessionId = sessionId };
        }

        return Ok(response);
    }


    [HttpPost("password/forgot")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.PasswordReset)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var response = await _serviceManager.AuthService.RequestPasswordResetAsync(request);
        // Always return 200 for security (don't reveal if email exists)
        return Ok(response);
    }


    [HttpPost("password/validate-token")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.PasswordReset)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ValidatePasswordResetToken([FromBody] ValidateResetTokenRequest request)
    {
        

        var response = await _serviceManager.AuthService.ValidatePasswordResetTokenAsync(request);

        if (!response.Successful)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }


    [HttpPost("password/reset")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.PasswordReset)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var response = await _serviceManager.AuthService.ResetPasswordAsync(request);

        if (!response.Successful)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }


    [HttpPost("password/verify")]
    [Authorize]
    public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordRequest request)
    {
        var response = await _serviceManager.AuthService.VerifyPasswordAsync(request);

        if (!response.Successful)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.RefreshTokenPolicy)]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        

        if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(ApiResponse<RefreshTokenResponse>.Failure("Both access token and refresh token are required"));
        }

        var response = await _serviceManager.AuthService.RefreshTokenAsync(request);

        if (!response.Successful)
        {
            // Check if it's an expired/invalid token vs other errors
            if (response.Message?.Contains("expired", StringComparison.OrdinalIgnoreCase) == true ||
                response.Message?.Contains("invalid", StringComparison.OrdinalIgnoreCase) == true ||
                response.Message?.Contains("revoked", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Unauthorized(response);
            }
            return BadRequest(response);
        }

        return Ok(response);
    }


    [HttpPost("logoutuser")]
    [Authorize]
    [EnableRateLimiting(RateLimitingPolicies.Api)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogoutUser()
    {
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();

        // Log the logout attempt
        _logger.LogInformation("Logout attempt for user: {UserId} from IP: {IpAddress}", userId, ipAddress);

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            await _serviceManager.SessionManagementService.EndSessionAsync(sessionId);
        }

        Response.Cookies.Delete("session_id");

        var response = await _serviceManager.AuthService.SignOutAsync();

        // Even if logout fails, return 200 for security
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting(RateLimitingPolicies.Api)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        
        User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            await _serviceManager.SessionManagementService.EndSessionAsync(sessionId);
        }

        var response = await _serviceManager.AuthService.SignOutAsync();

        return Ok(response);
    }

    [HttpPost("logout-all")]
    [Authorize]
    [EnableRateLimiting(RateLimitingPolicies.Api)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogoutAll()
    {
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentSessionId = Request.Headers["X-Session-Id"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            // End all user sessions except current one
            await _serviceManager.SessionManagementService.EndAllUserSessionsAsync(userId, currentSessionId);
        }

        var response = await _serviceManager.AuthService.SignOutAsync();

        return Ok(response);
    }


}
