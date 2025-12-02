using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Mvc.ViewModels.Profile;

public class ContactInformationViewModel
{
    [Display(Name = "Country/Region")]
    public string? CountryRegionCode { get; set; }

    [Display(Name = "Physical Address")]
    [StringLength(255, ErrorMessage = "Physical address cannot exceed 255 characters")]
    public string? PhysicalAddress { get; set; }

    [Display(Name = "Telephone Number")]
    [Phone(ErrorMessage = "Please enter a valid telephone number")]
    public string? TelephoneNo { get; set; }

    [Required(ErrorMessage = "Mobile number is required")]
    [Display(Name = "Mobile Number")]
    [Phone(ErrorMessage = "Please enter a valid mobile number")]
    public string MobileNo { get; set; } = string.Empty;

    [Display(Name = "Postal Address")]
    [StringLength(255, ErrorMessage = "Postal address cannot exceed 255 characters")]
    public string? PostalAddress { get; set; }

    [Display(Name = "Postal Code")]
    [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
    public string? PostCode { get; set; }

    [Display(Name = "City")]
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; set; }

    [Display(Name = "Contact Email Address")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string? ContactEMailAddress { get; set; }
}
