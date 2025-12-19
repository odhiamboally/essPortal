namespace EssPortal.Web.Blazor.Utilities.Constants;

public static class TwoFactorConstants
{
    public const int DefaultCodeLength = 6;
    public const int DefaultExpiryMinutes = 5;
    public const int MaxResendAttempts = 3;
    public const int ResendCooldownSeconds = 60;

    public static class Providers
    {
        public const string Email = "Email";
        public const string SMS = "SMS";
        public const string Phone = "Phone";
        public const string BackupCode = "BackupCode";
        public const string Authenticator = "Authenticator";
        public const string MicrosoftAuthenticator = "MicrosoftAuthenticator";
        public const string TOTP = "TOTP";
    }

    public static bool RequiresCodeSending(string? provider)
    {
        return provider?.ToLowerInvariant() switch
        {
            "email" => true,
            "phone" => true,
            "sms" => true,
            _ => false
        };
    }

    public static bool IsTotpProvider(string? provider)
    {
        return provider?.ToLowerInvariant() switch
        {
            "authenticator" => true,
            "microsoftauthenticator" => true,
            "totp" => true,
            _ => false
        };
    }
}
