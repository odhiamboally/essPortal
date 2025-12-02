namespace ESSPortal.Application.Dtos.TwoFactor;
public record BackupCodesInfo
{
    public List<string> BackupCodes { get; init; } = new();
}
