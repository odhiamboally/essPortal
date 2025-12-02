namespace ESSPortal.Application.Dtos.Auth;
public record RegisterEmployeeResponse(
    string UserId, 
    bool RequiresEmailConfirmation, 
    string ConfirmationLink,
    string Token
    );
