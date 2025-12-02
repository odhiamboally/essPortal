using ESSPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ESSPortal.Domain.Entities;


public class UserProfile : BaseEntity
{
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty; // FK to AppUser

    // Contact Information
    public string? CountryRegionCode { get; set; }
    public string? PhysicalAddress { get; set; }
    public string? TelephoneNo { get; set; }
    public string? MobileNo { get; set; }
    public string? PostalAddress { get; set; }
    public string? PostCode { get; set; }
    public string? City { get; set; }
    public string? ContactEMailAddress { get; set; }

    // Banking Information
    public string? BankAccountType { get; set; }
    public string? KBABankCode { get; set; }
    public string? KBABranchCode { get; set; }
    public string? BankAccountNo { get; set; }

    // Navigation Properties
    [JsonIgnore]
    public virtual AppUser User { get; set; } = null!;
}
