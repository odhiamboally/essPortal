using ESSPortal.Domain.Common;

namespace ESSPortal.Domain.Entities;
public class IpSecurityEvent : BaseEntity
{
    public string IpAddress { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? UserAgent { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}
