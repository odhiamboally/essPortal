namespace ESSPortal.Application.Constants;


public static class AuthErrorCodes
{
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string EMAIL_NOT_CONFIRMED = "EMAIL_NOT_CONFIRMED";
    public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";
    public const string TWO_FACTOR_REQUIRED = "TWO_FACTOR_REQUIRED";
    public const string INVALID_2FA_CODE = "INVALID_2FA_CODE";
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
    public const string INVALID_TOKEN = "INVALID_TOKEN";
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string SERVER_ERROR = "SERVER_ERROR";
    public const string RATE_LIMIT_EXCEEDED = "RATE_LIMIT_EXCEEDED";
}
