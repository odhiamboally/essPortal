using EssPortal.Web.Mvc.Enums.NavEnums;

namespace EssPortal.Web.Mvc.Models.Navision;

public class LeaveTypes
{
    public string? Key { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public decimal Days { get; set; }
    public bool DaysSpecified { get; set; }
    public bool AccrueDays { get; set; }
    public bool AccrueDaysSpecified { get; set; }
    public decimal ConversionRatePerDay { get; set; }
    public bool ConversionRatePerDaySpecified { get; set; }
    public bool UnlimitedDays { get; set; }
    public bool UnlimitedDaysSpecified { get; set; }
    public Gender Gender { get; set; }
    public bool GenderSpecified { get; set; }
    public Balance Balance { get; set; }
    public bool BalanceSpecified { get; set; }
    public decimal MaxCarryForwardDays { get; set; }
    public bool MaxCarryForwardDaysSpecified { get; set; }
    public bool AnnualLeave { get; set; }
    public bool AnnualLeaveSpecified { get; set; }
    public bool InclusiveOfHolidays { get; set; }
    public bool InclusiveOfHolidaysSpecified { get; set; }
    public bool InclusiveOfSaturday { get; set; }
    public bool InclusiveOfSaturdaySpecified { get; set; }
    public bool InclusiveOfSunday { get; set; }
    public bool InclusiveOfSundaySpecified { get; set; }
    public bool OffHolidaysDaysLeave { get; set; }
    public bool OffHolidaysDaysLeaveSpecified { get; set; }
    public LeaveTypeStatus Status { get; set; }
    public bool StatusSpecified { get; set; }

}
