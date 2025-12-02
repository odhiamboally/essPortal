using EssPortal.Domain.Enums.NavEnums;
using Microsoft.AspNetCore.Authentication;

namespace ESSPortal.Application.Dtos.Auth;
public record RegisterEmployeeRequest(
    string? EmployeeNumber,
    string? FirstName,           
    string? MiddleName,            
    string? LastName,            
    string? Email,               
    string? PhoneNumber,         
    Gender? Gender,              
    string? Password,
    string? ConfirmPassword,
    bool IsActive,
    bool IsDeleted,
    string? ReturnUrl,
    List<AuthenticationScheme> ExternalLogins
);

