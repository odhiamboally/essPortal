using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Blazor.ViewModels.Profile;

public class EmploymentDetailsViewModel
{
    [Display(Name = "Department")]
    public string? Department { get; set; }

    [Display(Name = "Job Title")]
    public string? JobTitle { get; set; }

    [Display(Name = "Manager")]
    public string? ManagerId { get; set; }

    [Display(Name = "Manager Name")]
    public string? ManagerName { get; set; }
}
