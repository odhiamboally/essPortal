using ESSPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ESSPortal.Domain.Entities;


public class UserSession : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTimeOffset LastAccessedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? EndedAt { get; set; }
    public string? EndReason { get; set; }

    [Required]
    [MaxLength(256)] // SHA-256 hash is 64 hex characters, so 256 is safe.
    public string DeviceFingerprint { get; set; } = string.Empty;

    // Navigation
    [JsonIgnore]
    public virtual AppUser User { get; set; } = null!;
}
