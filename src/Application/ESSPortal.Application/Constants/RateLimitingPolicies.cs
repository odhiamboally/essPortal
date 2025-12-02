namespace ESSPortal.Application.Constants;

public static class RateLimitingPolicies
{
    public const string Api = "ApiPolicy";
    public const string Authentication = "AuthPolicy";
    public const string Login = "Login";
    public const string PasswordReset = "PasswordResetPolicy";
    public const string TwoFactor = "TwoFactorPolicy";
    public const string FileUpload = "FileUploadPolicy";
    public const string SlidingWindow = "SlidingWindowPolicy";
    public const string TokenBucket = "TokenBucketPolicy";
    public const string RefreshTokenPolicy = "RefreshTokenPolicy";
}
