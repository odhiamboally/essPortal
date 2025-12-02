using EssPortal.Web.Mvc.Enums.NavEnums;

namespace ESSPortal.Web.Mvc.ViewModels.Leave;

public class LeaveApplicationListViewModel
{
    public string ApplicationNo { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public string? EmployeeNo { get; set; }
    public string? EmployeeName { get; set; }
    public decimal DaysApplied { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LeaveApplicationListStatus Status { get; set; }
    public string? LeavePeriod { get; set; }

    // Calculated properties
    public int Duration { get; set; }
    public bool IsCurrentYear { get; set; }
    public bool IsApproved { get; set; }
    public bool IsPending { get; set; }
    public bool IsRejected { get; set; }

    // Display properties for UN theme
    
    public string? DurationText { get; set; }
    public string? LeaveType { get; set; } 

    // UN Theme CSS classes
    public string? StatusCssClass { get; set; }
    public string? StateCssClass { get; set; }
    public string? StateText { get; set; }
    public string? StatusDisplayText { get; set; }
    public string? StatusDescription { get; internal set; }
}
