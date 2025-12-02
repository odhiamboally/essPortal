using ESSPortal.Application.Dtos.Common;
using ESSPortal.Domain.Entities;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IJwtService
{
    ApiResponse<JwtSecurityToken> GenerateToken(List<Claim> userClaims, TimeSpan timeSpan);
    Task<ApiResponse<string>> GenerateToken(AppUser user);
    ApiResponse<JwtSecurityToken> GetJwtToken(List<Claim> userClaims);
    ApiResponse<bool> IsTokenValid(SecurityToken token);
    ApiResponse<bool> IsTokenValid(string token);
    ApiResponse<bool> IsTokenExpired(JwtSecurityToken token);
    ApiResponse<string> GenerateRefreshToken(AppUser user);
    ApiResponse<bool> ValidateRefreshToken(string refreshToken, string userId);
    ApiResponse<string> GenerateTemporaryToken(List<Claim> claims, TimeSpan expiry);
    ApiResponse<DateTimeOffset> GetTokenExpiry(string token);
    ApiResponse<ClaimsPrincipal?> GetPrincipalFromToken(string token);
    ApiResponse<ClaimsPrincipal?> GetPrincipalFromExpiredToken(string token);
    
}
