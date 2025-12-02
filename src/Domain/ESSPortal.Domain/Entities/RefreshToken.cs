using ESSPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESSPortal.Domain.Entities;
public class RefreshToken : BaseEntity
{

    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? RevokedReason { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? TokenFamily { get; set; } = Guid.NewGuid().ToString();



    // Methods

    [NotMapped]
    public bool IsActive => !IsRevoked && !IsExpired;

    [NotMapped]
    public bool IsUsed => UsedAt.HasValue;

    [NotMapped]
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsRevoked => RevokedAt.HasValue;


    // Navigation

    [JsonIgnore]
    public virtual AppUser User { get; set; } = null!;
}
