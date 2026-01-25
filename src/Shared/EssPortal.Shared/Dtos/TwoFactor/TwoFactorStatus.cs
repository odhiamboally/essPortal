namespace ESSPortal.Shared.Dtos.TwoFactor;

public record TwoFactorStatus
{
    public bool IsEnabled { get; init; }
    public bool HasBackupCodes { get; init; }
}
