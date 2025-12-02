namespace ESSPortal.Web.Mvc.Dtos.TwoFactor;

public record BackupCodesInfo
{
    public List<string> BackupCodes { get; init; } = new();
}
