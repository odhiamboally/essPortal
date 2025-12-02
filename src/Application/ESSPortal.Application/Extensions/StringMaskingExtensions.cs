namespace ESSPortal.Application.Extensions;
public static class StringMaskingExtensions
{
    /// <summary>
    /// Masks a destination string based on the provider type
    /// </summary>
    public static string GetMaskedDestination(this string destination, string provider)
    {
        if (string.IsNullOrWhiteSpace(destination))
            return provider.ToLowerInvariant() switch
            {
                "email" => "***@***.com",
                "phone" or "sms" => "***-***-****",
                _ => "****"
            };

        return provider.ToLowerInvariant() switch
        {
            "email" => MaskEmail(destination),
            "phone" or "sms" => MaskPhoneNumber(destination),
            _ => "****"
        };
    }

    /// <summary>
    /// Masks an email address
    /// </summary>
    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return "***@***.com";

        var parts = email.Split('@');
        var username = parts[0];
        var domain = parts[1];

        var maskedUsername = username.Length <= 2
            ? new string('*', username.Length)
            : username[0] + new string('*', username.Length - 2) + username[^1];

        return $"{maskedUsername}@{domain}";
    }

    /// <summary>
    /// Masks a phone number
    /// </summary>
    private static string MaskPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return "***-***-****";

        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (digits.Length < 10)
            return "***-***-****";

        return digits.Length == 10
            ? $"***-***-{digits[^4..]}"
            : $"+***-***-***-{digits[^4..]}";
    }
}

