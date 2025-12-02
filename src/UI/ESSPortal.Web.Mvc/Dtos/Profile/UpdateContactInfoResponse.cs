namespace ESSPortal.Web.Mvc.Dtos.Profile;

public record UpdateContactInfoResponse(
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