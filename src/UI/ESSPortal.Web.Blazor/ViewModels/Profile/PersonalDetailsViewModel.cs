using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Blazor.ViewModels.Profile;

public class PersonalDetailsViewModel
{
    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "Middle Name")]
    [StringLength(50, ErrorMessage = "Middle name cannot exceed 50 characters")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Gender")]
    public string? Gender { get; set; }

    // Computed property
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
}
