namespace ESSPortal.Application.Dtos.TwoFactor;
public record VerifyBackupCodeRequest(string UserId, string Code);

