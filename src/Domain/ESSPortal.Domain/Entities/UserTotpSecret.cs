using ESSPortal.Domain.Common;

namespace ESSPortal.Domain.Entities;
public class UserTotpSecret : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string EncryptedSecret { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }

    // Navigation property
    public virtual AppUser User { get; set; } = null!;
}
