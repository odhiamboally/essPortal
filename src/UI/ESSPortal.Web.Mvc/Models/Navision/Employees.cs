using EssPortal.Web.Mvc.Enums.NavEnums;

namespace EssPortal.Web.Mvc.Models.Navision;

public class Employees
{
    public string? Key { get; set; }
    public string? No { get; set; }
    public string? FullName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? JobPositionTitle { get; set; }
    public string? Initials { get; set; }
    public string? JobTitle { get; set; }
    public string? PostCode { get; set; }
    public string? CountryRegionCode { get; set; }
    public string? PhoneNo { get; set; }
    public string? Extension { get; set; }
    public string? MobilePhoneNo { get; set; }
    public string? Email { get; set; }
    public string? StatisticsGroupCode { get; set; }
    public string? ResourceNo { get; set; }
    public bool PrivacyBlocked { get; set; }
    public bool PrivacyBlockedSpecified { get; set; }
    public string? SearchName { get; set; }
    public bool Comment { get; set; }
    public bool CommentSpecified { get; set; }
    public Gender Gender { get; set; }
    public bool GenderSpecified { get; set; }
    public string? PINNumber { get; set; }
    public string? IdNo { get; set; }
    public string? SocialSecurityNo { get; set; }
    public string? NHIFNo { get; set; }
    public Disabled Disabled { get; set; }
    public bool DisabledSpecified { get; set; }
    public Employment_Type EmploymentType { get; set; }
    public bool EmploymentTypeSpecified { get; set; }
    public EmployeesStatus Status { get; set; }
    public bool StatusSpecified { get; set; }
    public string? BankAccountNumber { get; set; }
}
