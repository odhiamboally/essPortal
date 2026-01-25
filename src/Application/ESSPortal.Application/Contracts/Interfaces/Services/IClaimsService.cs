using ESSPortal.Domain.Entities;
using ESSPortal.Shared.Dtos.Common;

using System.Security.Claims;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IClaimsService
{
    Task<AppResponse<List<Claim>>> GetUserClaimsAsync(AppUser appUser);
    Task<AppResponse<bool>> AddUserClaimAsync(AppUser user, Claim claim);
    Task<AppResponse<bool>> RemoveUserClaimAsync(AppUser user, Claim claim);
    Task<AppResponse<bool>> UpdateUserClaimAsync(AppUser user, Claim existingClaim, Claim newClaim);
}
