namespace ESSPortal.Shared.Dtos.TwoFactor;
public record VerifyBackupCodeRequest(string UserId, string Code);

