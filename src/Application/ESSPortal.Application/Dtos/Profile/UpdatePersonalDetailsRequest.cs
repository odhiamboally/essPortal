namespace ESSPortal.Application.Dtos.Profile;
public record UpdatePersonalDetailsRequest(
    string UserId,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? Gender
);
