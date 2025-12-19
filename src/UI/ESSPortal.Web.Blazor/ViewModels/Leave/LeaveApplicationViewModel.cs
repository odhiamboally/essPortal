using Microsoft.AspNetCore.Mvc.Rendering;

using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Blazor.ViewModels.Leave;

public class LeaveApplicationViewModel
{
    // Application Details
    public string ApplicationNo { get; set; } = string.Empty;

    [Display(Name = "Date of Application")]
    public DateTime ApplicationDate { get; set; } = DateTime.Now;

    [Display(Name = "Apply on Behalf")]
    public bool ApplyOnBehalf { get; set; }

    [Required]
    [Display(Name = "Employee No")]
    public string EmployeeNo { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Employee Name")]
    [StringLength(100)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string EmailAddress { get; set; } = string.Empty;

    [Display(Name = "Employment Type")]
    public string EmploymentType { get; set; } = string.Empty;

    [Display(Name = "Responsibility Center")]
    public string ResponsibilityCenter { get; set; } = string.Empty;

    [Required]
    [Phone]
    [Display(Name = "Mobile No")]
    public string MobileNo { get; set; } = string.Empty;

    [Display(Name = "Activity Code")]
    public string ActivityCode { get; set; } = string.Empty;

    // Leave Details
    [Display(Name = "Branch Code")]
    public string BranchCode { get; set; } = "NAIROBI";

    [Required]
    [Display(Name = "Leave Period")]
    public string LeavePeriod { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Leave Type")]
    public string LeaveType { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public string Status { get; set; } = "Open";

    [Display(Name = "Leave Earned to Date")]
    public decimal LeaveEarnedToDate { get; set; }

    [Required]
    [Range(0.5, 365, ErrorMessage = "Days applied must be between 0.5 and 365")]
    [Display(Name = "Days Applied")]
    public decimal DaysApplied { get; set; }

    [Required]
    [Display(Name = "From Date")]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [Required]
    [Display(Name = "To Date")]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    [Display(Name = "Resumption Date")]
    [DataType(DataType.Date)]
    public DateTime? ResumptionDate { get; set; }

    [Display(Name = "Leave Allowance Payable")]
    public bool HalfDay { get; set; }
    public bool LeaveAllowancePayable { get; set; }

    [Required]
    [Display(Name = "Reliever Employee No")]
    public string DutiesTakenOverBy { get; set; } = string.Empty;

    // Reason for Leave
    [Display(Name = "Reason for Leave")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string ReasonForLeave { get; set; } = string.Empty;

    // File Uploads
    [Display(Name = "Attachments")]
    public List<IFormFile> Attachments { get; set; } = [];

    public List<string> ExistingAttachments { get; set; } = new();

    // Dropdown Data Sources
    public List<SelectListItem> LeavePeriods { get; set; } = [];
    public List<SelectListItem> LeaveTypes { get; set; } = [];
    public List<SelectListItem> ActivityCodes { get; set; } = [];
    public List<SelectListItem> Employees { get; set; } = []; // For apply on behalf

    // Calculated Properties
    public int CalculatedBusinessDays
    {
        get
        {
            if (FromDate.HasValue && ToDate.HasValue)
            {
                return CalculateBusinessDays(FromDate.Value, ToDate.Value);
            }
            return 0;
        }
    }

    public DateTime? CalculatedResumptionDate
    {
        get
        {
            if (ToDate.HasValue)
            {
                var resumption = ToDate.Value.AddDays(1);

                // Skip weekends
                while (resumption.DayOfWeek == DayOfWeek.Saturday ||
                       resumption.DayOfWeek == DayOfWeek.Sunday)
                {
                    resumption = resumption.AddDays(1);
                }

                return resumption;
            }
            return null;
        }
    }

    public bool HasSufficientBalance => DaysApplied <= LeaveEarnedToDate;

    public decimal RemainingBalance => LeaveEarnedToDate - DaysApplied;

    // Helper Methods
    private int CalculateBusinessDays(DateTime startDate, DateTime endDate)
    {
        var businessDays = 0;
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            if (currentDate.DayOfWeek != DayOfWeek.Saturday &&
                currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
                businessDays++;
            }
            currentDate = currentDate.AddDays(1);
        }

        return businessDays;
    }

    // Validation Helper
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (FromDate.HasValue && FromDate.Value < DateTime.Today)
        {
            errors.Add("From date cannot be in the past");
        }

        if (FromDate.HasValue && ToDate.HasValue && ToDate.Value < FromDate.Value)
        {
            errors.Add("To date must be after from date");
        }

        if (DaysApplied > LeaveEarnedToDate)
        {
            errors.Add($"Cannot apply for more than {LeaveEarnedToDate} days (earned to date)");
        }

        if (CalculatedBusinessDays != (int)DaysApplied)
        {
            errors.Add("Days applied does not match the selected date range");
        }

        return errors;
    }
}
