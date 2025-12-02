using ESSPortal.Web.Mvc.Dtos.Auth;

using System.Security.Claims;

namespace ESSPortal.Web.Mvc.Mappings;

public static class ClaimMappingExtensions
{
    public static ClaimResponse ToDto(this Claim claim)
    {
        return new ClaimResponse(claim.Type, claim.Value);
    }

    public static Claim ToClaim(this ClaimResponse claimDto)
    {
        return new Claim(claimDto.Type, claimDto.Value);
    }

    public static List<ClaimResponse> ToDtoList(this IEnumerable<Claim> claims)
    {
        return claims.Select(c => c.ToDto()).ToList();
    }

    public static List<Claim> ToClaimList(this IEnumerable<ClaimResponse> claimDtos)
    {
        return [.. claimDtos.Select(c => c.ToClaim())];
    }
}
