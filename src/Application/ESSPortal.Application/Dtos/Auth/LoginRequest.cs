namespace ESSPortal.Application.Dtos.Auth;
public record LoginRequest(
    string EmployeeNumber,
    string Password,
    bool RememberMe,
    string? ReturnUrl,
    string DeviceFingerprint
);

