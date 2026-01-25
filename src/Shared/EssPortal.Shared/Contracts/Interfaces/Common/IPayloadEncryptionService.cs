namespace ESSPortal.Shared.Contracts.Interfaces.Common;

public interface IPayloadEncryptionService
{
    string Encrypt(string payload);
    string Decrypt(string encryptedPayload);
    bool IsEncrypted(string payload);
}
