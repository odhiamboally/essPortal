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


    
}
