namespace ESSPortal.Web.Mvc.Dtos.Profile;

public record UpdatePersonalDetailsResponse(
    string UserId,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? Gender
);