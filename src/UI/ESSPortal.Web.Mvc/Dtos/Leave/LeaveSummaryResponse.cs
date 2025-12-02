namespace ESSPortal.Web.Mvc.Dtos.Leave;

public record LeaveSummaryResponse
{
    public string EmployeeNo { get; init; } = string.Empty;
    public string CurrentPeriod { get; init; } = string.Empty;

    // Individual leave type statistics  
    public LeaveTypeStatistics AnnualLeave { get; init; } = new();
    public LeaveTypeStatistics AdoptionLeave { get; init; } = new();
    public LeaveTypeStatistics CompassionLeave { get; init; } = new();
    public LeaveTypeStatistics MaternityLeave { get; init; } = new();
    public LeaveTypeStatistics PaternityLeave { get; init; } = new();
    public LeaveTypeStatistics SickLeave { get; init; } = new();
    public LeaveTypeStatistics StudyLeave { get; init; } = new();
    public LeaveTypeStatistics UnpaidLeave { get; init; } = new();

    // Overall summary statistics
    public int TotalPendingApplications { get; init; }
    public int TotalApprovedThisMonth { get; init; }
    public decimal TotalMonthlyTaken { get; init; }

    // Quick access methods (same as before)
    public List<LeaveTypeStatistics> GetAllLeaveTypes() 
    {
        return
        [
            AnnualLeave,
            AdoptionLeave,
            CompassionLeave,
            MaternityLeave,
            PaternityLeave,
            SickLeave,
            StudyLeave,
            UnpaidLeave
        ];
    }
    public LeaveTypeStatistics GetLeaveType(string leaveCode) 
    {
        return leaveCode.ToUpperInvariant() switch
        {
            "ANNUAL" => AnnualLeave,
            "ADOPTION" => AdoptionLeave,
            "COMPASSIONATE" => CompassionLeave,
            "MATERNITY" => MaternityLeave,
            "PATERNITY" => PaternityLeave,
            "SICK" => SickLeave,
            "STUDY" => StudyLeave,
            "UNPAID" => UnpaidLeave,

            _ => new LeaveTypeStatistics()
        };
    }

    
}
