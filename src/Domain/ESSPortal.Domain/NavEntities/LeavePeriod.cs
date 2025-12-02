
using EssPortal.Domain.Enums.NavEnums;

namespace EssPortal.Domain.NavEntities;

public class LeavePeriod
{
    public string Key { get; set; } = string.Empty;
    public string? LeavePeriodName { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public bool StartDateSpecified { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public bool EndDateSpecified { get; set; }
    public bool Closed { get; set; }
    public bool ClosedSpecified { get; set; }
    public string? LeaveType { get; set; }
    public Employment_Type EmploymentType { get; set; }
    public bool EmploymentTypeSpecified { get; set; }
    public string? EmployeeNo { get; set; }

}
