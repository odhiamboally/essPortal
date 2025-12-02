using ESSPortal.Application.Contracts.Interfaces.Services;

using Microsoft.Extensions.Logging;

using OtpNet;

namespace ESSPortal.Infrastructure.Implementations.Services;
internal sealed class TotpService : ITotpService
{
    private readonly ILogger<TotpService> _logger;

    public TotpService(ILogger<TotpService> logger)
    {
        _logger = logger;
    }

    public string GenerateSecret()
    {
        // 20-byte (160-bit) secret key
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GenerateCode(string secret)
    {
        try
        {
            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes);
            return totp.ComputeTotp();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating TOTP code");
            throw;
        }
    }

    public string GenerateQrCodeUri(string email, string secret, string issuer = "UN SACCO ESS Portal")
    {
        try
        {
            var secretBytes = Base32Encoding.ToBytes(secret);

            // Generate standard TOTP URI
            var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?" +
                     $"secret={secret}&" +
                     $"issuer={Uri.EscapeDataString(issuer)}&" +
                     $"algorithm=SHA1&" +
                     $"digits=6&" +
                     $"period=30";

            return uri;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code URI for email: {Email}", email);
            throw;
        }
    }

    public bool VerifyTotpCode(string decryptedSecret, string code, int windowSize = 2)
    {
        _logger.LogInformation("Verifying TOTP: Secret={Secret}, Code={Code}", decryptedSecret, code);

        try
        {
            if (string.IsNullOrWhiteSpace(decryptedSecret) || string.IsNullOrWhiteSpace(code))
                return false;

            if (code.Length != 6)
                return false;

            code = code.Trim();
           
            byte[] secretBytes;

            try
            {
                secretBytes = Base32Encoding.ToBytes(decryptedSecret);  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decode Base32 secret.");
                return false;
            }
            
            var totp = new Totp(
                secretBytes,
                step: 30,
                mode: OtpHashMode.Sha1,
                totpSize: 6,
                timeCorrection: new TimeCorrection(DateTime.UtcNow));

            var verificationWindow = new VerificationWindow(previous: windowSize, future: windowSize);

            long timeStepMatched;
            var isValid = totp.VerifyTotp(code, out timeStepMatched, verificationWindow);

            _logger.LogDebug("TOTP verification - DecryptedSecret: {Secret}, Code: {Code}, Valid: {IsValid}, TimeStep: {TimeStep}",
            decryptedSecret, code, isValid, timeStepMatched);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying TOTP code");
            return false;
        }
    }

    public bool VerifyTotpCodePlainText(string plainSecret, string code, int windowSize = 2)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(plainSecret) || string.IsNullOrWhiteSpace(code))
                return false;

            if (code.Length != 6)
                return false;

            code = code.Trim();

            var secretBytes = Base32Encoding.ToBytes(plainSecret);

            var totp = new Totp(
                secretBytes,
                step: 30,
                mode: OtpHashMode.Sha1,
                totpSize: 6,
                timeCorrection: new TimeCorrection(DateTime.UtcNow));

            var currentCode = totp.ComputeTotp();
            _logger.LogDebug("Current expected code: {ExpectedCode}, Provided code: {ProvidedCode}",
                currentCode, code);

            var verificationWindow = new VerificationWindow(previous: windowSize, future: windowSize);

            long timeStepMatched;
            var isValid = totp.VerifyTotp(code, out timeStepMatched, verificationWindow);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying TOTP code with plain text secret");
            return false;
        }
    }

    public bool IsCodeValid(string secret, string code)
    {
        return VerifyTotpCode(secret, code);
    }
}