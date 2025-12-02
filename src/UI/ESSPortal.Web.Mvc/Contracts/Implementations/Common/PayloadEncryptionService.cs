using ESSPortal.Web.Mvc.Configurations;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;

using Jose;

using Microsoft.Extensions.Options;
using System.Text;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Common;

internal sealed class PayloadEncryptionService : IPayloadEncryptionService
{
    private readonly byte[] _key;
    private readonly ILogger<PayloadEncryptionService> _logger;
    private readonly PayloadEncryptionSettings _settings;

    public PayloadEncryptionService(IOptions<PayloadEncryptionSettings> settings, ILogger<PayloadEncryptionService> logger)
    {
        _logger = logger;
        _settings = settings.Value;

        if (string.IsNullOrWhiteSpace(_settings.Key))
        {
            _logger.LogWarning("Encryption key not configured, encryption will be disabled");
            _key = [];
            return;
        }

        _key = Encoding.UTF8.GetBytes(_settings.Key.PadRight(32)[..32]); // Ensure 32 bytes for AES-256
    }

    public string Encrypt(string payload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payload) || _key.Length == 0)
                return payload;

            var encryptedToken = JWE.Encrypt(payload, [new JweRecipient(JweAlgorithm.A256KW, _key)], JweEncryption.A256GCM);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedToken));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to encrypt payload, returning original");
            return payload; // Graceful degradation
        }
    }

    public string Decrypt(string encryptedPayload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(encryptedPayload) || _key.Length == 0)
            {
                _logger.LogWarning("Payload empty or key not configured");
                return encryptedPayload;
            }

            if (!IsEncrypted(encryptedPayload))
            {
                _logger.LogWarning("Payload doesn't look encrypted");
                return encryptedPayload;
            }

            var encryptedBytes = Convert.FromBase64String(encryptedPayload);
            var jweToken = Encoding.UTF8.GetString(encryptedBytes);

            var decryptedToken = JWE.Decrypt(jweToken, _key);
            return decryptedToken.Plaintext;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decrypt payload, returning original");
            return encryptedPayload; // Graceful degradation
        }
    }

    public bool IsEncrypted(string payload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payload))
                return false;

            // Check if it's valid base64 and contains JWE structure
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            var isJWE = decoded.Contains("\"ciphertext\"") &&
                       decoded.Contains("\"protected\"") &&
                       decoded.Contains("\"encrypted_key\"");

            _logger.LogDebug("IsEncrypted check - looks like JWE: {IsJWE}", isJWE);
            return isJWE;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "IsEncrypted check failed");
            return false;
        }
    }


}
