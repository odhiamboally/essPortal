using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Mvc.Dtos.Profile;

public class UpdateContactInfoRequest
{
    public string UserId { get; set; } = string.Empty;

    public string? CountryRegionCode { get; set; }

    [StringLength(255)]
    public string? PhysicalAddress { get; set; }

    [Phone]
    public string? TelephoneNo { get; set; }

    [Required(ErrorMessage = "Mobile number is required")]
    [Phone]
    public string MobileNo { get; set; } = string.Empty;

    [StringLength(255)]
    public string? PostalAddress { get; set; }

    [StringLength(20)]
    public string? PostCode { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [EmailAddress]
    public string? ContactEMailAddress { get; set; }
}
