using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Implementations.Services;
using ESSPortal.Application.Contracts.Interfaces.Common;

using Jose;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace ESSPortal.Application.Contracts.Implementations.Common;
internal sealed class PayloadEncryptionService : IPayloadEncryptionService
{
    private readonly byte[] _key;
    private readonly ILogger<EncryptionService> _logger;
    private readonly SecuritySettings _securitySettings;

    public PayloadEncryptionService(
       ILogger<EncryptionService> logger, 
       IOptions<SecuritySettings> securitySettings)
    {
        _logger = logger;
        _securitySettings = securitySettings.Value;

        if (string.IsNullOrWhiteSpace(_securitySettings.PayloadEncryption.Key))
            throw new InvalidOperationException("Encryption key not configured in SecuritySettings:PayloadEncryption:Key");

        _key = Encoding.UTF8.GetBytes(_securitySettings.PayloadEncryption.Key.PadRight(32)[..32]); // Ensure 32 bytes for AES-256
    }

    public string Encrypt(string payload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payload))
                return payload;

            var encryptedToken = JWE.Encrypt(payload, [new JweRecipient(JweAlgorithm.A256KW, _key)], JweEncryption.A256GCM);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedToken));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt payload");
            throw;
        }
    }

    public string Decrypt(string encryptedPayload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(encryptedPayload))
            {
                _logger.LogWarning("Payload is null/empty");
                return encryptedPayload;
            }

            if (!IsEncrypted(encryptedPayload))
            {
                _logger.LogWarning("Payload doesn't look encrypted, returning as-is");
                return encryptedPayload;
            }

            var encryptedBytes = Convert.FromBase64String(encryptedPayload);
            var jweToken = Encoding.UTF8.GetString(encryptedBytes);

            var jweDecrypted = JWE.Decrypt(jweToken, _key);
            return jweDecrypted.Plaintext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt payload");
            throw;
        }
    }

    public bool IsEncrypted_(string payload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payload))
                return false;

            // Check if it's valid base64 first
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(payload));

            // Check if the decoded content looks like our JWE JSON structure
            // Our JWE encryption creates a JSON object with these fields
            var isJWE = decoded.Contains("\"ciphertext\"") &&
                       decoded.Contains("\"protected\"") &&
                       decoded.Contains("\"encrypted_key\"");

            _logger.LogDebug("IsEncrypted check - payload length: {Length}, looks like JWE: {IsJWE}",
                payload.Length, isJWE);
            _logger.LogDebug("Decoded preview: {Preview}", decoded[..Math.Min(100, decoded.Length)]);

            return isJWE;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "IsEncrypted check failed");
            return false;
        }
    }

    public bool IsEncrypted(string payload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payload))
                return false;

            // First check if it's valid base64
            byte[] decodedBytes;
            try
            {
                decodedBytes = Convert.FromBase64String(payload);
            }
            catch (FormatException)
            {
                return false;
            }

            var decoded = Encoding.UTF8.GetString(decodedBytes);

            // Check if it looks like a JWE token (should start with eyJ and have 5 parts)
            // OR check if it's the JSON format that JWE.Encrypt might produce
            bool isJweCompact = decoded.StartsWith("eyJ") && decoded.Split('.').Length == 5;
            bool isJweJson = decoded.Contains("\"ciphertext\"") &&
                           decoded.Contains("\"protected\"") &&
                           decoded.Contains("\"encrypted_key\"");

            var result = isJweCompact || isJweJson;

            _logger.LogDebug("IsEncrypted check - payload length: {Length}, is JWE compact: {IsJweCompact}, is JWE JSON: {IsJweJson}, result: {Result}",
                payload.Length, isJweCompact, isJweJson, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "IsEncrypted check failed");
            return false;
        }
    }


}
