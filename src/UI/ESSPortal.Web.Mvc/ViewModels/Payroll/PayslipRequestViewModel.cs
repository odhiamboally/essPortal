using Microsoft.AspNetCore.Mvc.Rendering;

using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Mvc.ViewModels.Payroll;

public class PayslipRequestViewModel
{
    [Required(ErrorMessage = "Month is required")]
    [Display(Name = "Month")]
    [Range(1, 12, ErrorMessage = "Please select a valid month")]
    public int Month { get; set; } = DateTime.Now.Month;

    [Required(ErrorMessage = "Year is required")]
    [Display(Name = "Year")]
    [Range(2020, 2030, ErrorMessage = "Please select a valid year")]
    public int Year { get; set; } = DateTime.Now.Year;

    public string EmployeeNumber { get; set; } = string.Empty;

    // Display properties
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM");
    public string DisplayPeriod => $"{MonthName} {Year}";

    // Dropdown options
    public List<SelectListItem> AvailableMonths => GetMonthOptions();
    public List<SelectListItem> AvailableYears => GetYearOptions();

    private static List<SelectListItem> GetMonthOptions()
    {
        return Enumerable.Range(1, 12)
            .Select(month => new SelectListItem
            {
                Value = month.ToString(),
                Text = new DateTime(2024, month, 1).ToString("MMMM")
            })
            .ToList();
    }

    private static List<SelectListItem> GetYearOptions()
    {
        var currentYear = DateTime.Now.Year;
        return Enumerable.Range(currentYear - 5, 11)
            .Select(year => new SelectListItem
            {
                Value = year.ToString(),
                Text = year.ToString()
            })
            .ToList();
    }

    
}
