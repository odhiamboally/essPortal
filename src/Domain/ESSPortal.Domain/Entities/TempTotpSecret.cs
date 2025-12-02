using ESSPortal.Domain.Common;

namespace ESSPortal.Domain.Entities;
public class TempTotpSecret : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string EncryptedSecret { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }

    // Navigation property
    public virtual AppUser User { get; set; } = null!;
}
