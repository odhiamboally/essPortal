using Microsoft.AspNetCore.Mvc.Rendering;

using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Mvc.ViewModels.Payroll;

public class P9RequestViewModel
{
    [Required(ErrorMessage = "Year is required")]
    [Display(Name = "Tax Year")]
    [Range(2010, 2030, ErrorMessage = "Please select a valid year")]
    public int Year { get; set; } = DateTime.Now.Year;

    public string EmployeeNumber { get; set; } = string.Empty;

    // Auto-calculated period boundaries
    public DateTime StartDate => new(Year, 1, 1);
    public DateTime EndDate => new(Year, 12, 31);

    public string DisplayPeriod => $"January - December {Year}";

    // Dropdown options
    public List<SelectListItem> AvailableYears => GetYearOptions();

    private static List<SelectListItem> GetYearOptions()
    {
        var currentYear = DateTime.Now.Year;
        return Enumerable.Range(currentYear - 10, 11)
            .Select(year => new SelectListItem
            {
                Value = year.ToString(),
                Text = year.ToString()
            })
            .ToList();
    }
}
