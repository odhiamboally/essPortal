namespace ESSPortal.Application.Dtos.Profile;
public record UpdateContactInfoRequest(
    string UserId,
    string? CountryRegionCode,
    string? PhysicalAddress,
    string? TelephoneNo,
    string MobileNo,
    string? PostalAddress,
    string? PostCode,
    string? City,
    string? ContactEMailAddress
);
