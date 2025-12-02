namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string HashCode(string code);
    bool VerifyCode(string code, string hash);
}
