using ESSPortal.Domain.Common;

namespace ESSPortal.Domain.Entities;

public class BlockedIp : BaseEntity
{
    public string IpAddress { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset BlockedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? BlockedBy { get; set; }
}
