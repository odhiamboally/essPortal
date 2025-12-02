using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class ClaimsService : IClaimsService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ClaimsService> _logger;

    public ClaimsService(UserManager<AppUser> userManager, ILogger<ClaimsService> logger)
    {
        _userManager = userManager;
        _logger = logger;

    }

    public async Task<ApiResponse<bool>> AddUserClaimAsync(AppUser user, Claim claim)
    {
        try
        {
            var result = await _userManager.AddClaimAsync(user, claim);
            if (result.Succeeded)
            {
                _logger.LogInformation("Added claim - {ClaimType}:{ClaimValue} to user {UserId}", claim.Type, claim.Value, user.Id);
                    
                return ApiResponse<bool>.Success($"Added claim: {claim.Type}:{claim.Value} to user {user.Id}", true);
            }

            _logger.LogWarning("Failed to add claim {ClaimType}:{ClaimValue} to user {UserId}: {Errors}",
                claim.Type, claim.Value, user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));

            return ApiResponse<bool>.Failure($"Failed to add claim - {claim.Type}:{claim.Value} to user {user.Id}", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding claim to user {UserId}", user.Id);
            throw;
        }
    }

    public async Task<ApiResponse<List<Claim>>> GetUserClaimsAsync(AppUser appUser)
    {
        try
        {
            List<Claim> userClaims =
            [
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, appUser.Id),
                new Claim(ClaimTypes.Name, appUser.UserName!),
                new Claim(ClaimTypes.Email, appUser.Email!),
                new(JwtRegisteredClaimNames.Sub, appUser.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),

                //Custom claims
                new Claim("password", appUser.PasswordHash!),
                new Claim("firstname", appUser.FirstName!),
                new Claim("lastname", appUser.LastName!),
                new Claim("fullname", $"{appUser.FirstName} {appUser.LastName}".Trim()),
                new("employeenumber", appUser.EmployeeNumber ?? ""),
                new("emailconfirmed", appUser.EmailConfirmed.ToString().ToLowerInvariant()),
                new("phone_confirmed", appUser.PhoneNumberConfirmed.ToString().ToLowerInvariant()),
                new("twofactorenabled",
                    (await _userManager.GetTwoFactorEnabledAsync(appUser)).ToString().ToLowerInvariant()),
                new("accountcreated", appUser.CreatedAt.ToString("O") ?? DateTimeOffset.UtcNow.ToString("O")),
                new("lastlogin", appUser.LastLoginAt?.ToString("O") ?? DateTimeOffset.UtcNow.ToString("O"))
            ];

            if (!string.IsNullOrWhiteSpace(appUser.PhoneNumber))
            {
                userClaims.Add(new Claim(ClaimTypes.MobilePhone, appUser.PhoneNumber));
            }

            if (!string.IsNullOrWhiteSpace(appUser.Gender))
            {
                userClaims.Add(new Claim(ClaimTypes.Gender, appUser.Gender));
            }

            var userRoles = await _userManager.GetRolesAsync(appUser);
            userClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Add permissions (if you have a permission system)
            foreach (var role in userRoles)
            {
                var permissions = GetRolePermissions(role);
                userClaims.AddRange(permissions.Select(permission => new Claim("permission", permission)));
            }

            // Add user-specific claims from database
            var userSpecificClaims = await _userManager.GetClaimsAsync(appUser);
            userClaims.AddRange(userSpecificClaims);

            return ApiResponse<List<Claim>>.Success("Success", userClaims);

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ApiResponse<bool>> RemoveUserClaimAsync(AppUser user, Claim claim)
    {
        try
        {
            var result = await _userManager.RemoveClaimAsync(user, claim);
            if (result.Succeeded)
            {
                _logger.LogInformation("Removed claim {ClaimType}:{ClaimValue} from user {UserId}",
                    claim.Type, claim.Value, user.Id);

                return ApiResponse<bool>.Success($"Removed claim: {claim.Type}:{claim.Value} from user {user.Id}", true);
            }

            _logger.LogWarning("Failed to remove claim {ClaimType}:{ClaimValue} from user {UserId}: {Errors}",
                claim.Type, claim.Value, user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));

            return ApiResponse<bool>.Failure($"Failed to remove claim - {claim.Type}:{claim.Value} from user {user.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing claim from user {UserId}", user.Id);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> UpdateUserClaimAsync(AppUser user, Claim existingClaim, Claim newClaim)
    {
        try
        {
            // Remove old claim and add new one
            var removeResult = await _userManager.RemoveClaimAsync(user, existingClaim);
            if (!removeResult.Succeeded)
            {
                _logger.LogWarning("Failed to remove existing claim for user {UserId}", user.Id);
                return ApiResponse<bool>.Failure($"Failed to remove claim - {existingClaim.Type}:{existingClaim.Value} from user {user.Id}");
            }

            var addResult = await _userManager.AddClaimAsync(user, newClaim);
            if (!addResult.Succeeded)
            {
                // Try to rollback by adding the old claim back
                await _userManager.AddClaimAsync(user, existingClaim);
                _logger.LogWarning("Failed to add new claim for user {UserId}, rolled back", user.Id);
                return ApiResponse<bool>.Failure($"Failed to add new claim - {newClaim.Type}:{newClaim.Value} to user {user.Id} - rolled back");
            }

            _logger.LogInformation("Updated claim for user {UserId}: {OldClaim} -> {NewClaim}",
                user.Id, $"{existingClaim.Type}:{existingClaim.Value}", $"{newClaim.Type}:{newClaim.Value}");

            return ApiResponse<bool>.Success($"Updated claim claim: {newClaim.Type}:{newClaim.Value} for user {user.Id}", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating claim for user {UserId}", user.Id);
            throw;
        }
    }

    private List<string> GetRolePermissions(string roleName)
    {
        return roleName switch
        {
            "Admin" => ["user.create", "user.read", "user.update", "user.delete",
                    "role.create", "role.read", "role.update", "role.delete",
                    "system.settings", "reports.view", "audit.view"],

            "Manager" => ["user.read", "user.update", "reports.view", "team.manage"],

            "Employee" => ["profile.read", "profile.update", "documents.read"],
            _ => []
        };
    }




}
