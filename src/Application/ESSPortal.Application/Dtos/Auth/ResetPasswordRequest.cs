namespace ESSPortal.Application.Dtos.Auth;
public record ResetPasswordRequest
{
    public string Email { get; init; } = string.Empty;
    public string? NewPassword { get; init; }  
    public string? Password { get; init; }    
    public string? ConfirmPassword { get; init; }
    public string Token { get; init; } = string.Empty;
    public string? EmployeeNumber { get; init; }
    public string? Code { get; init; }
    public string? LogoBase64 { get; init; }
}