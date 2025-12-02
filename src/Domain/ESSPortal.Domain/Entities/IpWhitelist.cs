using ESSPortal.Domain.Common;

namespace ESSPortal.Domain.Entities;

public class IpWhitelist : BaseEntity
{
    public string IpAddress { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsAdminWhitelist { get; set; } = false;
    public string? AddedBy { get; set; }
}
