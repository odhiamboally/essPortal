namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Common;

public interface IPayloadEncryptionService
{
    string Encrypt(string payload);
    string Decrypt(string encryptedPayload);
    bool IsEncrypted(string payload);
}
