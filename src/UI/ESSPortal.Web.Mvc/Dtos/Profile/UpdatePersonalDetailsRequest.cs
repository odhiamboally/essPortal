using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Mvc.Dtos.Profile;

public class UpdatePersonalDetailsRequest
{
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    public string? Gender { get; set; }
}
