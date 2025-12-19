using System;
using System.Collections.Generic;
using System.Text;

namespace ESSPortal.Application.Dtos.Employee;

public class EmployeeCardResponse
{
    public string? No { get; set; }

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? OtherName { get; set; }

    public string? JobTitle { get; set; }
    public string? JobPosition { get; set; }
    public string? JobPositionTitle { get; set; }
    public string? SecondaryJobPosition { get; set; }
    public string? SecondaryJobPositionTitle { get; set; }
    public string? ResponsibilityCenter { get; set; }

    public string? Initials { get; set; }
    public string? SearchName { get; set; }
    public string? Gender { get; set; }

    public string? PhoneNo2 { get; set; }
    public string? MobilePhoneNo { get; set; }
    public string? Extension { get; set; }
    public string? Pager { get; set; }

    public string? Email { get; set; }
    public string? CompanyEmail { get; set; }

    public bool PrivacyBlocked { get; set; }
    public bool PrivacyBlockedSpecified { get; set; }

    public string? UserId { get; set; }
    public string? ManagerSupervisor { get; set; }

    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public string? PostCode { get; set; }
    public string? CountryRegionCode { get; set; }

    public string? EmploymentType { get; set; }
    public string? ContractType { get; set; }
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }

    public DateTime? EmploymentDate { get; set; }
    public DateTime? BirthDate { get; set; }

    public string? Status { get; set; }
    public DateTime? TerminationDate { get; set; }

    public string? IdNo { get; set; }
    public string? PinNumber { get; set; }
    public string? NhifNo { get; set; }
    public string? SocialSecurityNo { get; set; }

    public string? BankAccountNumber { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankBranch { get; set; }
    public string? BankName { get; set; }

    public decimal? BasicPay { get; set; }
    public decimal? HouseAllowance { get; set; }
    public decimal? TotalAllowances { get; set; }
    public decimal? TotalDeductions { get; set; }

    public DateTime? LastDateModified { get; set; }
}
