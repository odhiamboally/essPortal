using EssPortal.Domain.Enums.NavEnums;

using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Domain.Entities;
using ESSPortal.Domain.NavEntities;

using System;
using System.Collections.Generic;
using System.Text;

namespace ESSPortal.Application.Mappings;

public static class LeaveMappingExtensions
{
    public static Domain.Entities.LeaveApplicationCard ToLeaveApplicationCard(this CreateLeaveApplicationCardRequest req)
    {
        return new Domain.Entities.LeaveApplicationCard
        {
            Application_No = req.Application_No,
            Application_Date = req.Application_Date,
            Apply_on_behalf = req.Apply_on_behalf,

            Employee_No = req.Employee_No,
            Employee_Name = req.Employee_Name,
            Employment_Type = req.Employment_Type,
            Responsibility_Center = req.Responsibility_Center,
            Mobile_No = req.Mobile_No,

            Shortcut_Dimension_1_Code = req.Shortcut_Dimension_1_Code,
            Shortcut_Dimension_2_Code = req.Shortcut_Dimension_2_Code,

            Leave_Period = req.Leave_Period,
            Leave_Code = req.Leave_Code,
            Leave_Status = req.Leave_Status,
            Status = req.Status,

            Leave_Earned_to_Date = req.Leave_Earned_to_Date,
            Days_Applied = req.Days_Applied,

            Start_Date = req.Start_Date,
            End_Date = req.End_Date,
            Resumption_Date = req.Resumption_Date,

            Duties_Taken_Over_By = req.Duties_Taken_Over_By,
            Relieving_Name = req.Relieving_Name,

            Leave_Allowance_Payable = req.Leave_Allowance_Payable,
            Email_Adress = req.Email_Adress
        };
    }

    public static Domain.Entities.LeaveApplicationCard ToLeaveApplicationCard(this LeaveApplicationCardResponse response)
    {
        return new Domain.Entities.LeaveApplicationCard
        {
            Application_No = response.ApplicationNo,
            Application_Date = response.ApplicationDate ?? default,
            Apply_on_behalf = response.ApplyOnBehalf,

            Employee_No = response.EmployeeNo,
            Employee_Name = response.EmployeeName,
            Employment_Type = response.EmploymentType,
            Responsibility_Center = response.ResponsibilityCenter,
            Mobile_No = response.MobileNo,

            Shortcut_Dimension_1_Code = response.ShortcutDimension1Code,
            Shortcut_Dimension_2_Code = response.ShortcutDimension2Code,

            Leave_Period = response.LeavePeriod,
            Leave_Code = response.LeaveCode,
            Leave_Status = response.LeaveStatus,
            Status = GetLeaveApplicationCardStatusDescription(response.Status),

            Leave_Earned_to_Date = response.LeaveEarnedToDate,
            Days_Applied = response.DaysApplied,

            Start_Date = response.StartDate,
            End_Date = response.EndDate,
            Resumption_Date = response.ResumptionDate,

            Duties_Taken_Over_By = response.DutiesTakenOverBy,
            Relieving_Name = response.RelievingName,

            Leave_Allowance_Payable = response.LeaveAllowancePayable,
            Email_Adress = response.EmailAddress
        };
    }

    public static List<Domain.Entities.LeaveApplicationCard> ToLeaveApplicationCards(this List<LeaveApplicationCardResponse> entities)
    {
        return entities.Select(ToLeaveApplicationCard).ToList();
    }


    public static LeaveApplicationCardResponse ToLeaveApplicationCardResponse(this Domain.NavEntities.LeaveApplication.LeaveApplicationCard card)
    {
        return new LeaveApplicationCardResponse
        {
            ApplicationNo = card.applicationNo ?? string.Empty,
            EmployeeNo = card.employeeNumber,
            ApplyOnBehalf = card.applyOnBehalf ?? false,
            LeaveCode = card.leaveCode,
            StartDate = card.leaveStartDate.ToDateTime(TimeOnly.MinValue),
            DaysApplied = card.daysApplied,
            DutiesTakenOverBy = card.dutiesTakenOverBy
        };
    }

    public static LeaveApplicationCardResponse ToLeaveApplicationCardResponseExtended(this Domain.Entities.LeaveApplicationCard card)
    {
        return new LeaveApplicationCardResponse
        {
            ApplicationNo = card.Application_No,
            ApplicationDate = card.Application_Date,

            ApplyOnBehalf = card.Apply_on_behalf ?? false,
            ApplyOnBehalfSpecified = card.Apply_on_behalf.HasValue,

            EmployeeNo = card.Employee_No,
            EmployeeName = card.Employee_Name,
            EmploymentType = card.Employment_Type ?? string.Empty,
            ResponsibilityCenter = card.Responsibility_Center,
            MobileNo = card.Mobile_No,

            ShortcutDimension1Code = card.Shortcut_Dimension_1_Code,
            ShortcutDimension2Code = card.Shortcut_Dimension_2_Code,

            LeavePeriod = card.Leave_Period,
            LeaveCode = card.Leave_Code,
            LeaveStatus = card.Leave_Status,
            Status = ParseLeaveApplicationCardStatus(card.Status ?? string.Empty),

            LeaveEarnedToDate = card.Leave_Earned_to_Date!.Value,
            DaysApplied = card.Days_Applied,

            StartDate = card.Start_Date,
            EndDate = card.End_Date,
            ResumptionDate = card.Resumption_Date,

            DutiesTakenOverBy = card.Duties_Taken_Over_By,
            RelievingName = card.Relieving_Name,

            LeaveAllowancePayable = card.Leave_Allowance_Payable ?? false,
            LeaveAllowancePayableSpecified = card.Leave_Allowance_Payable.HasValue,

            EmailAddress = card.Email_Adress
        };
    }

    public static LeaveApplicationList ToLeaveApplicationList(this CreateLeaveApplicationListRequest request)
    {
        return new LeaveApplicationList
        {
            Employee_No = request.EmployeeNo,
            Start_Date = request.StartDate,
            End_Date = request.EndDate,
            Leave_Period = request.LeavePeriod
        };
    }

    public static LeaveApplicationList ToLeaveApplicationList(this LeaveApplicationListResponse response)
    {
        return new LeaveApplicationList
        {
            Employee_No = response.EmployeeNo,
            Start_Date = response.StartDate,
            End_Date = response.EndDate,
            Leave_Period = response.LeavePeriod
        };
    }

    public static List<LeaveApplicationList> ToLeaveApplicationLists(this List<LeaveApplicationListResponse> entities)
    {
        return entities.Select(ToLeaveApplicationList).ToList();
    }


    public static LeaveApplicationListResponse ToLeaveApplicationListResponse(this LeaveApplicationList entity)
    {
        return new LeaveApplicationListResponse
        {
            ApplicationNo = entity.Application_No,
            ApplicationDate = entity.Application_Date,
            EmployeeNo = entity.Employee_No,
            EmployeeName = entity.Employee_Name,
            StartDate = entity.Start_Date,
            EndDate = entity.End_Date,
            DaysApplied = entity.Days_Applied ?? 0,
            LeavePeriod = entity.Leave_Period ?? string.Empty,
            Status = Enum.TryParse<LeaveApplicationListStatus>(
                entity.Status,
                true,
                out var status)
                    ? status
                    : LeaveApplicationListStatus.Open,

            LeaveType = entity.Leave_Period ?? string.Empty
        };
    }

    public static IEnumerable<LeaveApplicationListResponse> ToLeaveApplicationListResponses(this IEnumerable<LeaveApplicationList> entities)
    {
        return entities.Select(ToLeaveApplicationListResponse);
    }

    public static LeaveRelievers ToLeaveRelievers(this CreateLeaveRelieverRequest request)
    {
        return new LeaveRelievers
        {
            Application_No = request.ApplicationNo,
            Staff_Name = request.StaffName,
            Leave_Code = request.LeaveCode
        };
    }

    public static LeaveRelieverResponse ToLeaveRelieverResponse(this LeaveRelievers reliever)
    {
        return new LeaveRelieverResponse
        {
            EmployeeNo = string.Empty, // Not provided by NAV entity
            EmployeeName = reliever.Staff_Name ?? string.Empty,
            EmailAddress = string.Empty,
            Department = string.Empty,
            JobTitle = string.Empty
        };
    }

    public static LeaveRelieverResponse ToLeaveRelieverResponse(this LeaveRelievers reliever, EmployeeCard employee)
    {
        return new LeaveRelieverResponse
        {
            EmployeeNo = employee.No ?? string.Empty,
            EmployeeName = reliever.Staff_Name ?? $"{employee.First_Name} {employee.Last_Name}",
            EmailAddress = employee.Company_E_Mail ?? string.Empty,
            Department = employee.Global_Dimension_1_Code ?? string.Empty,
            JobTitle = employee.Job_Title ?? string.Empty
        };
    }

    public static IEnumerable<LeaveRelieverResponse> ToLeaveRelieverResponses(this IEnumerable<LeaveRelievers> entities)
    {
        return entities.Select(ToLeaveRelieverResponse);
    }

    public static LeaveStatisticsFactboxResponse ToLeaveStatisticsFactboxResponse(this LeaveStatisticsFactbox factbox)
    {
        return new LeaveStatisticsFactboxResponse
        {
            Key = factbox.Key,

            LeaveEntitlement = factbox.Leave_Entitlment,
            LeaveEntitlementSpecified = factbox.Leave_EntitlmentSpecified,

            LeaveEarnedToDate = factbox.LeaveEarnedToDate,
            LeaveEarnedToDateSpecified = factbox.LeaveEarnedToDateSpecified,

            RecalledDays = factbox.Recalled_Days,
            RecalledDaysSpecified = factbox.Recalled_DaysSpecified,

            DaysAbsent = factbox.Days_Absent,
            DaysAbsentSpecified = factbox.Days_AbsentSpecified,

            BalanceBroughtForward = factbox.Balance_brought_forward,
            BalanceBroughtForwardSpecified = factbox.Balance_brought_forwardSpecified,

            TotalLeaveDaysTaken = factbox.Total_Leave_Days_Taken,
            TotalLeaveDaysTakenSpecified = factbox.Total_Leave_Days_TakenSpecified,

            LeaveBalance = factbox.Leave_Balance,
            LeaveBalanceSpecified = factbox.Leave_BalanceSpecified
        };
    }

    public static IEnumerable<LeaveStatisticsFactboxResponse> ToLeaveStatisticsFactboxResponses(this IEnumerable<LeaveStatisticsFactbox> entities)
    {
        return entities.Select(ToLeaveStatisticsFactboxResponse);
    }

    public static LeaveTypes ToLeaveType(this CreateLeaveTypeRequest request)
    {
        return new LeaveTypes
        {
            Code = request.Code,
            Description = request.Description,
            Days = request.Days,
            Accrue_Days = request.Accrue_Days,
            Conversion_Rate_Per_Day = request.Conversion_Rate_Per_Day,
            Unlimited_Days = request.Unlimited_Days,
            Gender = request.Gender,
            Balance = request.Balance,
            Max_Carry_Forward_Days = request.Max_Carry_Forward_Days,
            Annual_Leave = request.Annual_Leave,
            Inclusive_of_Holidays = request.Inclusive_of_Holidays,
            Inclusive_of_Saturday = request.Inclusive_of_Saturday,
            Inclusive_of_Sunday = request.Inclusive_of_Sunday,
            Off_Holidays_Days_Leave = request.Off_Holidays_Days_Leave,
            Status = request.Status
        };
    }

    public static LeaveTypes ToLeaveType(this LeaveTypeResponse request)
    {
        return new LeaveTypes
        {
            Code = request.Code,
            Description = request.Description,
            Days = request.Days,
            Accrue_Days = request.AccrueDays,
            Conversion_Rate_Per_Day = request.ConversionRatePerDay,
            Unlimited_Days = request.UnlimitedDays,
            Gender = request.Gender,
            Balance = request.Balance,
            Max_Carry_Forward_Days = request.MaxCarryForwardDays,
            Annual_Leave = request.AnnualLeave,
            Inclusive_of_Holidays = request.InclusiveOfHolidays,
            Inclusive_of_Saturday = request.InclusiveOfSaturday,
            Inclusive_of_Sunday = request.InclusiveOfSunday,
            Off_Holidays_Days_Leave = request.OffHolidaysDaysLeave,
            Status = request.Status
        };
    }

    public static List<LeaveTypes> ToLeaveTypes(this List<LeaveTypeResponse> entities)
    {
        return entities.Select(ToLeaveType).ToList();
    }

    public static LeaveTypeResponse ToLeaveTypeResponse(this LeaveTypes leaveType)
    {
        return new LeaveTypeResponse
        {
            Code = leaveType.Code,
            Description = leaveType.Description,
            Days = leaveType.Days,
            AccrueDays = leaveType.Accrue_Days,
            ConversionRatePerDay = leaveType.Conversion_Rate_Per_Day,
            UnlimitedDays = leaveType.Unlimited_Days,
            Gender = leaveType.Gender,
            Balance = leaveType.Balance,
            MaxCarryForwardDays = leaveType.Max_Carry_Forward_Days,
            AnnualLeave = leaveType.Annual_Leave,
            InclusiveOfHolidays = leaveType.Inclusive_of_Holidays,
            InclusiveOfSaturday = leaveType.Inclusive_of_Saturday,
            InclusiveOfSunday = leaveType.Inclusive_of_Sunday,
            OffHolidaysDaysLeave = leaveType.Off_Holidays_Days_Leave,
            Status = leaveType.Status
        };
    }

    public static IEnumerable<LeaveTypeResponse> ToLeaveTypeResponses(this IEnumerable<LeaveTypes> entities)
    {
        return entities.Select(ToLeaveTypeResponse);
    }



    private static LeaveApplicationCardStatus ParseLeaveApplicationCardStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return LeaveApplicationCardStatus.Open;

        return status.ToUpperInvariant() switch
        {
            "RELEASED" => LeaveApplicationCardStatus.Released,
            "PENDING APPROVAL" => LeaveApplicationCardStatus.Pending_Approval,
            "PENDING_APPROVAL" => LeaveApplicationCardStatus.Pending_Approval,
            "PENDING_PREPAYMENT" => LeaveApplicationCardStatus.Pending_Prepayment,
            "PENDING PREPAYMENT" => LeaveApplicationCardStatus.Pending_Prepayment,
            "REJECTED" => LeaveApplicationCardStatus.Rejected,
            "OPEN" => LeaveApplicationCardStatus.Open,
            _ => LeaveApplicationCardStatus.Open
        };
    }

    private static string GetLeaveApplicationCardStatusDescription(LeaveApplicationCardStatus status)
    {
        return status switch
        {
            LeaveApplicationCardStatus.Released => "Approved",
            LeaveApplicationCardStatus.Open => "Draft",
            LeaveApplicationCardStatus.Pending_Approval => "Pending Approval",
            LeaveApplicationCardStatus.Pending_Prepayment => "Pending Payment",
            LeaveApplicationCardStatus.Rejected => "Rejected",
            _ => "Under Review"
        };
    }


}
