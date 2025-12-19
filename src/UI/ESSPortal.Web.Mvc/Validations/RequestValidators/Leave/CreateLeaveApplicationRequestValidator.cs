using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Dtos.Leave;
using ESSPortal.Web.Mvc.Extensions;

using FluentValidation;

namespace ESSPortal.Web.Mvc.Validations.RequestValidators.Leave;

public class CreateLeaveApplicationRequestValidator : AbstractValidator<CreateLeaveApplicationRequest>
{
    private readonly IServiceManager _serviceManager;
    private readonly string? _gender;

    private readonly Dictionary<string, LeaveTypeConstraints> _leaveTypeConstraints = new()
    {
        { "ANNUAL", new LeaveTypeConstraints 
            { 
                MaxDays = 30, 
                MonthlyEarned = 2.5m, 
                YearlyEntitlement = 30, 
                RequiresEarnedDays = true 
            } 
        },
        { "ADOPTION", new LeaveTypeConstraints 
            { 
                MaxDays = 5, 
                MonthlyEarned = 5, 
                YearlyEntitlement = 5, 
                RequiresEarnedDays = false 
            } 
        },
        { "COMPASSIONATE", new LeaveTypeConstraints 
            { 
                MaxDays = 5, 
                MonthlyEarned = 5, 
                YearlyEntitlement = 5, 
                RequiresEarnedDays = false 
            } 
        },
        { "MATERNITY", new LeaveTypeConstraints 
            { 
                MaxDays = 90, 
                MonthlyEarned = 90, 
                YearlyEntitlement = 90, 
                RequiresEarnedDays = false 
            } 
        },
        { "PATERNITY", new LeaveTypeConstraints 
            { 
                MaxDays = 14, 
                MonthlyEarned = 14, 
                YearlyEntitlement = 14, 
                RequiresEarnedDays = false 
            } 
        },
        { "SICK", new LeaveTypeConstraints 
            { 
                MaxDays = 90, 
                MonthlyEarned = 90, 
                YearlyEntitlement = 90, 
                RequiresEarnedDays = false 
            } 
        },
        { "STUDY", new LeaveTypeConstraints 
            { 
                MaxDays = 10, 
                MonthlyEarned = 10, 
                YearlyEntitlement = 10, 
                RequiresEarnedDays = false 
            } 
        },
        { "UNPAID", new LeaveTypeConstraints 
            { 
                MaxDays = 60, 
                MonthlyEarned = 60, 
                YearlyEntitlement = 60, 
                RequiresEarnedDays = false 
            } 
        }
    };
    

    public CreateLeaveApplicationRequestValidator(IServiceManager serviceManager, string gender, bool isEditing) 
    {
        
        _serviceManager = serviceManager;
        _gender = gender;


        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        // Basic field validation
        RuleFor(x => x.EmployeeNo)
            .NotEmpty()
            .WithMessage("Employee number is required");

        RuleFor(x => x.EmployeeName)
            .NotEmpty()
            .WithMessage("Employee name is required");

        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Valid email address is required");

        RuleFor(x => x.MobileNo)
            .NotEmpty()
            .WithMessage("Mobile number is required")
            .Matches(@"^[+]?[0-9\s\-\(\)]{6,20}$")
            .WithMessage("Please enter a valid international mobile number");

        RuleFor(x => x.LeavePeriod)
            .NotEmpty()
            .WithMessage("Leave period is required");

        RuleFor(x => x.LeaveType)
            .NotEmpty()
            .WithMessage("Leave type is required")
            .Must(BeValidLeaveType)
            .WithMessage("Invalid leave type selected");

        RuleFor(x => x.DaysApplied)
            .GreaterThan(0)
            .WithMessage("Days applied must be greater than 0")
            .Must(BeInHalfDayIncrements)
            .WithMessage("Days must be in 0.5 increments (e.g., 1, 1.5, 2)")
            .LessThanOrEqualTo(365)
            .WithMessage("Days applied cannot exceed 365");
        
        if (!isEditing)
        {
            RuleFor(x => x.FromDate)
            .NotEmpty()
            .WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Start date cannot be in the past");

        }

        RuleFor(x => x.ToDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("End date must be after or same as start date");

        // Single reliever validation
        RuleFor(x => x.SelectedRelieverEmployeeNos)
            .NotEmpty()
            .WithMessage("Please select a reliever for your leave")
            .Must(HaveOnlyOneReliever)
            .WithMessage("Please select only one reliever");

        // Leave type specific validations
        RuleFor(x => x)
            .Must(NotExceedLeaveTypeMaximum)
            .WithMessage(x => GetLeaveTypeMaximumMessage(x.LeaveType))
            .WithName("DaysApplied");

        RuleFor(x => x)
            .MustAsync(HaveSufficientLeaveBalance)
            .WithMessage(x => GetInsufficientBalanceMessage(x.LeaveType))
            .WithName("DaysApplied");

        RuleFor(x => x)
            .Must(HaveReasonableDuration)
            .WithMessage(x => GetReasonableDurationMessage(x.LeaveType))
            .WithName("Duration");

        RuleFor(x => x)
            .Must(HaveValidHalfDayConfiguration)
            .WithMessage("Half-day leave must be exactly 0.5 days with same start and end date")
            .When(x => x.HalfDay)
            .WithName("HalfDay");

        RuleFor(x => x)
            .Must(HaveValidDateRange)
            .WithMessage("Invalid date range selected");

        // Leave allowance validation (only for annual leave)
        RuleFor(x => x)
            .MustAsync(MeetLeaveAllowanceRequirements)
            .WithMessage("Leave allowance is only available for annual leave when you have taken 10 or more days in total.")
            .When(x => x.LeaveAllowancePayable)
            .WithName("LeaveAllowancePayable");

        // Gender-specific validation for maternity/paternity
        RuleFor(x => x)
            .Must(MeetGenderRequirements)
            .WithMessage(x => GetGenderRequirementMessage(x.LeaveType))
            .When(x => x.LeaveType == "MATERNITY" || x.LeaveType == "PATERNITY")
            .WithName("LeaveType");

    }


    private class LeaveTypeConstraints
    {
        public decimal? MaxDays { get; set; }
        public decimal MonthlyEarned { get; set; }
        public decimal YearlyEntitlement { get; set; }
        public bool RequiresEarnedDays { get; set; }
    }

    private bool BeValidLeaveType(string leaveType)
    {
        return !string.IsNullOrEmpty(leaveType) && _leaveTypeConstraints.ContainsKey(leaveType.ToUpperInvariant());
    }

    private bool HaveOnlyOneReliever(List<string> relievers)
    {
        return relievers is [_];
    }

    private bool NotExceedLeaveTypeMaximum(CreateLeaveApplicationRequest request)
    {
        if (string.IsNullOrEmpty(request.LeaveType) ||
            !_leaveTypeConstraints.TryGetValue(request.LeaveType.ToUpperInvariant(), out var constraints))
            return true; // Let other validators handle invalid leave type

        // Check against leave type specific maximum
        if (constraints.MaxDays.HasValue && request.DaysApplied > constraints.MaxDays.Value)
            return false;

        return true;
    }

    private bool HaveValidHalfDayConfiguration(CreateLeaveApplicationRequest request)
    {
        if (!request.HalfDay) return true; 

        if (request.DaysApplied != 0.5m) return false;

        if (request.FromDate.Date != request.ToDate.Date) return false;

        return true;
    }

    private async Task<bool> HaveSufficientLeaveBalance(CreateLeaveApplicationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.LeaveType) ||
                !_leaveTypeConstraints.TryGetValue(request.LeaveType.ToUpperInvariant(), out var constraints))
                return false;

            var leaveSummary = await GetLeaveSummary(request.EmployeeNo);
            if (leaveSummary == null) return false;

            var leaveType = request.LeaveType.ToUpperInvariant();

            // For annual leave, check against earned days and current balance
            if (leaveType == "ANNUAL")
            {
                var hasEarnedDays = request.DaysApplied <= leaveSummary.GetLeaveType(leaveType).EarnedToDate;
                var hasCurrentBalance = request.DaysApplied <= leaveSummary.GetLeaveType(leaveType).CurrentBalance;
                return hasEarnedDays && hasCurrentBalance;
            }

            // For other leave types, check against yearly entitlement minus already taken
            // We'll need to get leave type specific statistics here
            // For now, assume they can take up to the yearly entitlement
            var yearlyEntitlement = constraints.YearlyEntitlement;

            // TODO: Implement leave type specific balance checking
            // This should check how much of this specific leave type has been taken this year
            // For now, allow if within yearly entitlement
            return request.DaysApplied <= yearlyEntitlement;
        }
        catch (Exception)
        {
            return true; // Allow BC to handle validation if we can't validate here
        }
    }

    private async Task<bool> MeetLeaveAllowanceRequirements(CreateLeaveApplicationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!request.LeaveAllowancePayable)
                return true;

            // Leave allowance is only for annual leave
            if (request.LeaveType?.ToUpperInvariant() != "ANNUAL")
                return false;

            var leaveSummary = await GetLeaveSummary(request.EmployeeNo);
            if (leaveSummary == null) return false;

            // Calculate total qualifying days = current year taken + new application days
            var currentDaysTaken = leaveSummary.GetLeaveType("ANNUAL").TotalTaken;
            var totalQualifyingDays = currentDaysTaken + request.DaysApplied;

            // BC Rule: Must have 10 or more total days to be eligible for leave allowance
            const decimal minimumQualifyingDays = 10m;

            return totalQualifyingDays >= minimumQualifyingDays;
        }
        catch (Exception)
        {
            // Don't allow validation to pass silently on errors
            return true;
        }
    }

    private bool MeetGenderRequirements(CreateLeaveApplicationRequest request)
    {
        if (string.IsNullOrEmpty(request.LeaveType) || string.IsNullOrEmpty(request.EmployeeNo))
            return false;

        var leaveType = request.LeaveType.ToUpperInvariant();

        if (_leaveTypeConstraints.TryGetValue(leaveType, out _))
        {
            if ((leaveType == "MATERNITY" && request.DaysApplied > 90) ||
                (leaveType == "PATERNITY" && request.DaysApplied > 14))
            {
                return false;
            }
        }

        if (leaveType == "MATERNITY")
        {
            if (!string.Equals(_gender, "F", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(_gender, "FEMALE", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        else if (leaveType == "PATERNITY")
        {
            if (!string.Equals(_gender, "M", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(_gender, "MALE", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private bool HaveValidDateRange(CreateLeaveApplicationRequest request)
    {
        if (request.FromDate == default || request.ToDate == default)
            return false;

        var maxFutureDate = DateTime.Today.AddYears(1);
        if (request.FromDate > maxFutureDate)
            return false;

        if (request.ToDate > maxFutureDate)
                        return false;

        return true;
    }

    private bool HaveReasonableDuration(CreateLeaveApplicationRequest request)
    {
        if (string.IsNullOrEmpty(request.LeaveType) ||
        !_leaveTypeConstraints.TryGetValue(request.LeaveType.ToUpperInvariant(), out _))
            return true;

        var consecutiveDays = (request.ToDate - request.FromDate).TotalDays + 1;
        var leaveType = request.LeaveType.ToUpperInvariant();

        var maxConsecutiveDays = leaveType switch
        {
            // Calendar day leave types
            "SICK" => 90,           
            "MATERNITY" => 90,       
            "PATERNITY" => 14,      
            "ADOPTION" => 5,        
            "COMPASSIONATE" => 5,   

            // Business day leave types - need to be more generous with calendar days
            // since weekends don't count toward the leave balance
            "ANNUAL" => 45,         // ~30 business days = ~45 calendar days (accounting for weekends)
            "STUDY" => 15,          // ~10 business days = ~15 calendar days
            "UNPAID" => 90,         // 60 business days = ~90 calendar days

            _ => 90                 // Default
        };

        return consecutiveDays <= maxConsecutiveDays;
    }

    private bool BeInHalfDayIncrements(decimal days)
    {
        return days * 2 % 1 == 0; // Check if it's in 0.5 increments
    }

    private async Task<LeaveSummaryResponse?> GetLeaveSummary(string employeeNo)
    {
        var cachedDashboardData = _serviceManager.CacheService.GetDashboard(employeeNo);

        if (cachedDashboardData?.LeaveSummary != null)
        {
            return cachedDashboardData.LeaveSummary;
        }

        var leaveSummaryResponse = await _serviceManager.LeaveService.GetLeaveSummaryAsync(employeeNo);
        if (!leaveSummaryResponse.Successful || leaveSummaryResponse.Data == null)
        {
            return null;
        }

        _serviceManager.CacheService.SetLeaveSummary(employeeNo, leaveSummaryResponse.Data);
        return leaveSummaryResponse.Data;
    }

    private string GetLeaveTypeMaximumMessage(string leaveType)
    {
        if (string.IsNullOrEmpty(leaveType) || !_leaveTypeConstraints.TryGetValue(leaveType.ToUpperInvariant(), out var constraints))
            return "Invalid leave type selected";
        return constraints.MaxDays.HasValue
            ? $"You cannot apply for more than {constraints.MaxDays.Value} days of {leaveType} leave at a time."
            : "This leave type does not have a maximum day limit.";
    }

    private string GetInsufficientBalanceMessage(string leaveType)
    {
        if (string.IsNullOrEmpty(leaveType) || !_leaveTypeConstraints.TryGetValue(leaveType.ToUpperInvariant(), out _))
            return "Invalid leave type selected";
        return leaveType.ToUpperInvariant() switch
        {
            "ANNUAL" => "You do not have enough annual leave balance to apply for this duration.",
            "ADOPTION" => "You do not have enough adoption leave balance to apply for this duration.",
            "COMPASSIONATE" => "You do not have enough compassionate leave balance to apply for this duration.",
            "MATERNITY" => "You do not have enough maternity leave balance to apply for this duration.",
            "PATERNITY" => "You do not have enough paternity leave balance to apply for this duration.",
            "SICK" => "You do not have enough sick leave balance to apply for this duration.",
            "STUDY" => "You do not have enough study leave balance to apply for this duration.",
            "UNPAID" => "You cannot apply for unpaid leave without sufficient balance.",
            _ => "You do not have enough leave balance to apply for this duration."
        };
    }

    private string GetGenderRequirementMessage(string leaveType)
    {
        return leaveType?.ToUpperInvariant() switch
        {
            "MATERNITY" => "Maternity leave can only be requested by female employees.",
            "PATERNITY" => "Paternity leave can only be requested by male employees.",
            _ => "Gender requirements not met for this leave type."
        };
    }

    private string GetReasonableDurationMessage(string leaveType)
    {
        if (string.IsNullOrEmpty(leaveType) || !_leaveTypeConstraints.TryGetValue(leaveType.ToUpperInvariant(), out _))
            return "Invalid leave type selected";

        var (maxDays, dayType) = leaveType?.ToUpperInvariant() switch
        {
            "SICK" => (90, "calendar"),
            "MATERNITY" => (90, "calendar"),
            "PATERNITY" => (14, "calendar"),
            "ADOPTION" => (5, "calendar"),
            "COMPASSIONATE" => (5, "calendar"),
            "ANNUAL" => (45, "consecutive"), // Don't specify business/calendar to avoid confusion
            "STUDY" => (15, "consecutive"),
            "UNPAID" => (90, "consecutive"),
            _ => (90, "consecutive")
        };

        return $"Leave duration cannot exceed {maxDays} consecutive {dayType} days for {leaveType} leave";
    }



}
