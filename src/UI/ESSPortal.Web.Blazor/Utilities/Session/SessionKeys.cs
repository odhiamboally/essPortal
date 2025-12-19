namespace ESSPortal.Web.Blazor.Utilities.Session;

public static class SessionKeys
{
    public const string IsLocked = "IsLocked";
    public const string LockedAt = "LockedAt";
    public const string LockReturnUrl = "LockReturnUrl";

    // 2FA Session Keys
    public const string TwoFA_UserId = "2FA_UserId";
    public const string TwoFA_Provider = "2FA_Provider";
    public const string TwoFA_SentAt = "2FA_SentAt";
    public const string TwoFA_ExpiresAt = "2FA_ExpiresAt";

    // User Session Keys
    public const string UserProfile = "UserProfile";
    public const string UserInfo = "UserInfo";
    public const string SessionId = "SessionId";
    public const string UserId = "UserId";

    // Cookie Names
    public const string AuthTokenCookie = "auth_token";
    public const string RefreshTokenCookie = "refresh_token";
    public const string SessionIdCookie = "session_id";
    public const string AspNetCoreCookie = ".AspNetCore.Cookies";
    public const string ESSAuthCookie = "ESS_Auth";
    public const string ESSSessionCookie = "ESS_Session";
}
