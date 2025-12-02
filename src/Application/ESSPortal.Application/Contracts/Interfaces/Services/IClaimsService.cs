using ESSPortal.Application.Dtos.Common;
using ESSPortal.Domain.Entities;
using System.Security.Claims;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IClaimsService
{
    Task<ApiResponse<List<Claim>>> GetUserClaimsAsync(AppUser appUser);
    Task<ApiResponse<bool>> AddUserClaimAsync(AppUser user, Claim claim);
    Task<ApiResponse<bool>> RemoveUserClaimAsync(AppUser user, Claim claim);
    Task<ApiResponse<bool>> UpdateUserClaimAsync(AppUser user, Claim existingClaim, Claim newClaim);
}
