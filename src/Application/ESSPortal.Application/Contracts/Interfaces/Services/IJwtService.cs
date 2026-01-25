using ESSPortal.Domain.Entities;
using ESSPortal.Shared.Dtos.Common;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IJwtService
{
    AppResponse<JwtSecurityToken> GenerateToken(List<Claim> userClaims, TimeSpan timeSpan);
    Task<AppResponse<string>> GenerateToken(AppUser user);
    AppResponse<JwtSecurityToken> GetJwtToken(List<Claim> userClaims);
    AppResponse<bool> IsTokenValid(SecurityToken token);
    AppResponse<bool> IsTokenValid(string token);
    AppResponse<bool> IsTokenExpired(JwtSecurityToken token);
    AppResponse<string> GenerateRefreshToken(AppUser user);
    AppResponse<bool> ValidateRefreshToken(string refreshToken, string userId);
    AppResponse<string> GenerateTemporaryToken(List<Claim> claims, TimeSpan expiry);
    AppResponse<DateTimeOffset> GetTokenExpiry(string token);
    AppResponse<ClaimsPrincipal?> GetPrincipalFromToken(string token);
    AppResponse<ClaimsPrincipal?> GetPrincipalFromExpiredToken(string token);
    
}
