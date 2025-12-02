using ESSPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Domain.Entities;
public class Profile : BaseEntity
{
    [MaxLength(450)]
    public string? UserId { get; set; }
    public string? Emp_No { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? No { get; set; }
    public string? CountryRegionCode { get; set; }
    public string? PhysicalAddress { get; set; }
    public string? TelephoneNo { get; set; }
    public string? MobileNo { get; set; }
    public string? PostalAddress { get; set; }
    public string? PostCode { get; set; }
    public string? City { get; set; }
    public string? ContactEMailAddress { get; set; }
    public string? BankAccountType { get; set; }
    public string? KBABankCode { get; set; }
    public string? KBABranchCode { get; set; }
    public string? BankAccountNo { get; set; }
    public DateTimeOffset? DateCreated { get; set; }

    public virtual AppUser? User { get; set; }
    public List<Upload> Uploads { get; set; } = [];
}
