namespace ESSPortal.Application.Dtos.Profile;
public record UserInfo(
    string UserId,
    string IDNo,
    string? EmployeeNumber,
    string? FirstName,
    string? LastName,
    string? Email,
    string? PhoneNumber,
    string ResponsibilityCenter,
    string JobPositionTitle,
    string ManagerSupervisor,
    string EmploymentType,
    string? ProfilePictureUrl,
    string? CountryRegionCode,
    bool EmailConfirmed,
    bool PhoneNumberConfirmed,
    bool TwoFactorEnabled,
    DateTimeOffset? LastLoginAt,
    List<string> Roles
);
