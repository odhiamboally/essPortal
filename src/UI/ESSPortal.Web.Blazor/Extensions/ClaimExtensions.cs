using ESSPortal.Web.Blazor.Dtos.Auth;
using System.Security.Claims;

namespace ESSPortal.Web.Blazor.Extensions;

public static class ClaimExtensions
{
    public static List<Claim> ToClaims(this List<UserClaimsResponse>? claimDtos)
    {
        if (claimDtos == null || !claimDtos.Any())
            return new List<Claim>();

        return claimDtos.Select(dto =>
        {
            var claim = new Claim(dto.Type, dto.Value, dto.ValueType, dto.Issuer, dto.OriginalIssuer);

            // Add properties
            foreach (var property in dto.Properties)
            {
                claim.Properties[property.Key] = property.Value;
            }

            return claim;
        }).ToList();
    }
}
