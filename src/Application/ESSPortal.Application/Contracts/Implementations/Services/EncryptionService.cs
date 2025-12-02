using ESSPortal.Application.Contracts.Interfaces.Services;

using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class EncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;

    public EncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("TotpSecrets");
    }

    public string Encrypt(string plainText)
    {
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        return _protector.Unprotect(cipherText);
    }

    public string HashCode(string code)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyCode(string code, string hash)
    {
        var codeHash = HashCode(code);
        return codeHash.Equals(hash, StringComparison.Ordinal);
    }
}
