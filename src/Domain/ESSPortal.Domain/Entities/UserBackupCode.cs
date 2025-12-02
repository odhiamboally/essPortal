using ESSPortal.Domain.Common;

namespace ESSPortal.Domain.Entities;
public class UserBackupCode : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string HashedCode { get; set; } = string.Empty;
    public bool IsUsed { get; set; } = false;
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    // Navigation property
    public virtual AppUser User { get; set; } = null!;
}
