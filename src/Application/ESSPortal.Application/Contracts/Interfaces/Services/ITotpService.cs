namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ITotpService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string email, string secret, string issuer = "UN SACCO ESS Portal");
    bool VerifyTotpCode(string secret, string code, int windowSize = 1);
    bool VerifyTotpCodePlainText(string plainSecret, string code, int windowSize = 2);
    string GenerateCode(string secret);
    bool IsCodeValid(string secret, string code);
}
