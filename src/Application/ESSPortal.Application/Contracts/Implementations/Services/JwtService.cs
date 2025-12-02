using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Utilities;
using ESSPortal.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<AppUser> _userManager;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly ILogger<JwtService> _logger;

    public JwtService(
        IOptions<JwtSettings> jwtSettings, 
        UserManager<AppUser> userManager, 
        TokenValidationParameters tokenValidationParameters,
        ILogger<JwtService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _userManager = userManager;
        _tokenValidationParameters = tokenValidationParameters;
        _logger = logger;
    }



    public async Task<ApiResponse<string>> GenerateToken(AppUser user)
    {
        var now = DateTime.UtcNow;
        var key = _jwtSettings.GetSymmetricSecurityKey();
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);

        //var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.NameIdentifier, user.Id),
        //    new Claim(ClaimTypes.Email, user.Email!),
        //    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
        //    new Claim("EmployeeNumber", user.EmployeeNumber!),
        //    new("token_type", "access"),
        //    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            
        //};

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new Claim("EmployeeNumber", user.EmployeeNumber!),
            new("token_type", "access"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                    

        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return ApiResponse<string>.Success("Access token generated successfully", new JwtSecurityTokenHandler().WriteToken(token));
    }

    public ApiResponse<JwtSecurityToken> GenerateToken(List<Claim> userClaims, TimeSpan timeSpan)
    {
        try
        {
            AuthExtensions.SecurityKey(out _);

            var authSigningKey = _jwtSettings.GetSymmetricSecurityKey();
            var credentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256);

            var expiry = DateTime.UtcNow.Add(
                timeSpan == default
                    ? TimeSpan.FromMinutes(_jwtSettings.AccessTokenExpiryMinutes)
                    : timeSpan
            );

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: userClaims,
                notBefore: DateTime.UtcNow,
                expires: expiry,
                signingCredentials: credentials
            );

            return ApiResponse<JwtSecurityToken>.Success("JWT generated successfully", token);

        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating JWT token: {ex.Message}", ex);
        }
    }

    public ApiResponse<JwtSecurityToken> GetJwtToken(List<Claim> userClaims)
    {
        var defaultExpiry = TimeSpan.FromMinutes(_jwtSettings.AccessTokenExpiryMinutes);
        return GenerateToken(userClaims, defaultExpiry);
    }

    public ApiResponse<string> GenerateTemporaryToken(List<Claim> claims, TimeSpan expiry)
    {
        try
        {
            var tempClaims = claims.ToList();
            tempClaims.Add(new Claim("token_type", "temporary"));

            var jwtTokenResponse = GenerateToken(tempClaims, expiry);
            var jwtToken = jwtTokenResponse.Data;
            return ApiResponse<string>.Success("Temp token generated successfully", new JwtSecurityTokenHandler().WriteToken(jwtToken));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating temporary token: {ex.Message}", ex);
        }
    }

    public ApiResponse<string> GenerateRefreshToken(AppUser user)
    {
        try
        {
            var now = DateTime.UtcNow;
            var key = _jwtSettings.GetSymmetricSecurityKey();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var refreshClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new("EmployeeNumber", user.EmployeeNumber ?? string.Empty),
                new("token_type", "refresh"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)

            };

            var refreshToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: refreshClaims,
                notBefore: now,
                expires: now.AddHours(_jwtSettings.RefreshTokenExpiryHours),
                signingCredentials: credentials
            );

            return ApiResponse<string>.Success("Refresh token generated successfully", new JwtSecurityTokenHandler().WriteToken(refreshToken));
        }
        catch (Exception)
        {
            throw;
        }
    }

    public ApiResponse<ClaimsPrincipal?> GetPrincipalFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = _tokenValidationParameters.Clone();
            validationParameters.ValidateLifetime = false; // Don't validate expiry for refresh scenarios

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return ApiResponse<ClaimsPrincipal?>.Failure("Invalid token", null);

            return ApiResponse<ClaimsPrincipal?>.Success("ClaimsPrincipal retreived successfully", principal);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public ApiResponse<ClaimsPrincipal?> GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token is null or empty");
                return ApiResponse<ClaimsPrincipal?>.Failure("Token is null or empty");
            }

            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            {
                _logger.LogWarning("JWT Security Key is not configured");
                return ApiResponse<ClaimsPrincipal?>.Failure("JWT Security Key is not configured");

            }

            var tokenHandler = new JwtSecurityTokenHandler();

            // Create token validation parameters with NO expiry validation
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false, // IMPORTANT: Don't validate expiry
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Verify it's a JWT token
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid JWT algorithm or token type");
                return ApiResponse<ClaimsPrincipal?>.Failure("Failed to retreive principal");
            }

            return ApiResponse<ClaimsPrincipal?>.Success("Principal retrieved sucessfully", principal);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Security token exception while parsing expired token");
            return ApiResponse<ClaimsPrincipal?>.Failure("Failed to retreive principal");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing expired token");
            return ApiResponse<ClaimsPrincipal?>.Failure("Failed to retreive principal");
        }
    }

    public ApiResponse<DateTimeOffset> GetTokenExpiry(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            return ApiResponse<DateTimeOffset>.Success(
                "Token Expiry",
                DateTimeOffset.FromUnixTimeSeconds(jsonToken.Payload.Expiration ?? 0) 
            );
        }
        catch (Exception)
        {
            throw;
        }
    }

    public ApiResponse<bool> IsTokenValid(SecurityToken token)
    {
        try
        {
            if (token == null)
                throw new ArgumentException("Token is null");

            AuthExtensions.SecurityKey(out _);
            var clockSkew = Convert.ToDouble(_jwtSettings.ClockSkew);

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _jwtSettings.GetSymmetricSecurityKey(),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(clockSkew)

            };

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            new JwtSecurityTokenHandler().ValidateToken(jwtSecurityTokenHandler.WriteToken(token), tokenValidationParameters, out _);

            return ApiResponse<bool>.Success("Valid", true);
        }
        catch (SecurityTokenExpiredException)
        {
            return ApiResponse<bool>.Failure("Token has expired.");
        }
        catch (SecurityTokenValidationException ex)
        {
            return ApiResponse<bool>.Failure($"Invalid token: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Unexpected error during token validation: {ex.Message}");
        }
    }

    public ApiResponse<bool> IsTokenValid(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token is null");

            AuthExtensions.SecurityKey(out _);
            var tokenHandler = new JwtSecurityTokenHandler();
            var clockSkew = Convert.ToDouble(_jwtSettings.ClockSkew);

            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _jwtSettings.GetSymmetricSecurityKey(),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(clockSkew)

            };

            tokenHandler.ValidateToken(token, tokenValidationParameters, out _);

            return ApiResponse<bool>.Success("Valid", true);
        }
        catch (SecurityTokenExpiredException)
        {
            return ApiResponse<bool>.Failure("Token has expired.");
        }
        catch (SecurityTokenValidationException ex)
        {
            return ApiResponse<bool>.Failure($"Invalid token: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Unexpected error during token validation: {ex.Message}");
        }
    }

    public ApiResponse<bool> IsTokenExpired(JwtSecurityToken token)
    {
        if (token.ValidTo > DateTime.UtcNow)
        {
            return ApiResponse<bool>.Success("Not Expired", true);
        }
        
        return ApiResponse<bool>.Failure("Token is expired", false);
    }

    public ApiResponse<bool> ValidateRefreshToken(string refreshToken, string userId)
    {
        try
        {
            var principal = GetPrincipalFromToken(refreshToken);
            if (!principal.Successful || principal.Data == null)
                return ApiResponse<bool>.Failure($"Error validating refresh token. | {principal.Message}");

            var tokenUserId = principal.Data.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenType = principal.Data.FindFirst("token_type")?.Value;

            var isValid = tokenUserId == userId && tokenType == "refresh";
            if (!isValid)
                return ApiResponse<bool>.Failure("Invalid refresh token for the user");

            // Optionally, you can also check if the token is expired
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(refreshToken);
            var isExpired = IsTokenExpired(jwtToken).Data;
            if (isExpired)
                return ApiResponse<bool>.Failure("Refresh token is expired");

            return ApiResponse<bool>.Success("Refresh token validation successful", isValid);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<List<Claim>> GetUserClaims(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("employee_number", user.EmployeeNumber ?? ""),
            new("first_name", user.FirstName ?? ""),
            new("last_name", user.LastName ?? ""),
            new("email_confirmed", user.EmailConfirmed.ToString()),
            new("phone_confirmed", user.PhoneNumberConfirmed.ToString()),
            new("two_factor_enabled", (await _userManager.GetTwoFactorEnabledAsync(user)).ToString()),
            new("last_login", DateTimeOffset.UtcNow.ToString("O")) // Current login time
        };

        // Add roles
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Add custom claims if any
        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        return claims;
    }
}

