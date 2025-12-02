using EssPortal.Domain.Enums.NavEnums;
using ESSPortal.Application.Dtos.ModelFilters;

namespace EssPortal.Application.Dtos.ModelFilters;

public record EmployeesFilter : BaseFilter
{
    public string? No { get; init; }
    public string? FullName { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? JobPositionTitle { get; init; }
    public string? Initials { get; init; }
    public string? JobTitle { get; init; }
    public string? PostCode { get; init; }
    public string? CountryRegionCode { get; init; }
    public string? PhoneNo { get; init; }
    public string? Extension { get; init; }
    public string? MobilePhoneNo { get; init; }
    public string? Email { get; init; }
    public string? StatisticsGroupCode { get; init; }
    public string? ResourceNo { get; init; }
    public bool? PrivacyBlocked { get; init; }
    public string? SearchName { get; init; }
    public string? Comment { get; init; }
    public Gender? Gender { get; init; }
    public string? PinNumber { get; init; }
    public string? IdNo { get; init; }
    public string? SocialSecurityNo { get; init; }
    public string? NhifNo { get; init; }
    public Disabled? Disabled { get; init; }
    public Employment_Type? EmploymentType { get; init; }
    public EmployeesStatus? Status { get; init; }
    public string? BankAccountNumber { get; init; }


    //public Dictionary<string, string?> CustomQueryParameters()
    //{
    //   var parameters = new Dictionary<string, string?>();

    //   void AddIf(string key, string? value)
    //   {
    //      if (!string.IsNullOrWhiteSpace(value))
    //         parameters[key] = value;
    //   }

    //   // Explicit mapping for all fields
    //   AddIf(nameof(No), No);
    //   AddIf(nameof(FullName), FullName);
    //   AddIf(nameof(FirstName), FirstName);
    //   AddIf(nameof(MiddleName), MiddleName);
    //   AddIf(nameof(LastName), LastName);
    //   AddIf(nameof(JobPositionTitle), JobPositionTitle);
    //   AddIf(nameof(Initials), Initials);
    //   AddIf(nameof(JobTitle), JobTitle);
    //   AddIf(nameof(PostCode), PostCode);
    //   AddIf(nameof(CountryRegionCode), CountryRegionCode);
    //   AddIf(nameof(PhoneNo), PhoneNo);
    //   AddIf(nameof(Extension), Extension);
    //   AddIf(nameof(MobilePhoneNo), MobilePhoneNo);
    //   AddIf(nameof(Email), Email);
    //   AddIf(nameof(StatisticsGroupCode), StatisticsGroupCode);
    //   AddIf(nameof(ResourceNo), ResourceNo);
    //   AddIf(nameof(PrivacyBlocked), PrivacyBlocked?.ToString());
    //   AddIf(nameof(SearchName), SearchName);
    //   AddIf(nameof(Comment), Comment);
    //   AddIf(nameof(Gender), Gender.ToString());
    //   AddIf(nameof(PinNumber), PinNumber);
    //   AddIf(nameof(IdNo), IdNo);
    //   AddIf(nameof(SocialSecurityNo), SocialSecurityNo);
    //   AddIf(nameof(NhifNo), NhifNo);
    //   AddIf(nameof(Disabled), Disabled?.ToString());
    //   AddIf(nameof(EmploymentType), EmploymentType.ToString());
    //   AddIf(nameof(Status), Status.ToString());
    //   AddIf(nameof(BankAccountNumber), BankAccountNumber);

    //   return parameters;
    //}
}
