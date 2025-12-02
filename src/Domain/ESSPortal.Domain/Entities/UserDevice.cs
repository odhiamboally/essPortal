using ESSPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ESSPortal.Domain.Entities;
public class UserDevice : BaseEntity
{
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public bool IsTrusted { get; set; } = false;
    public DateTimeOffset? TrustedUntil { get; set; }

    // Navigation

    [JsonIgnore]
    public virtual AppUser User { get; set; } = null!;
}
