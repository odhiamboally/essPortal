using Microsoft.IdentityModel.Tokens;

namespace ESSPortal.Web.Mvc.Configurations;

public class JwtSettings
{
    public string? Secret { get; set; }
    public string? SecretKey { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int AccessTokenExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryHours { get; set; } = 8;
    public int ClockSkew { get; set; }
    public SymmetricSecurityKey? SignKey { get; set; }
}
