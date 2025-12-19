using ESSPortal.Web.Blazor.ViewModels.Leave;

using FluentValidation;

namespace ESSPortal.Web.Blazor.Validations.RequestValidators.Leave;

public class LeaveApplicationViewModelValidator : AbstractValidator<LeaveApplicationViewModel>
{
    private readonly Dictionary<string, LeaveTypeConstraints> _leaveTypeConstraints = new()
    {
        { "ANNUAL",       new LeaveTypeConstraints { MaxDays = 30,   MonthlyEarned = 2.5m, YearlyEntitlement = 30,  RequiresEarnedDays = true } },
        { "ADOPTION",     new LeaveTypeConstraints { MaxDays = 5,    MonthlyEarned = 5,    YearlyEntitlement = 5,   RequiresEarnedDays = false } },
        { "COMPASSIONATE",new LeaveTypeConstraints { MaxDays = 5,    MonthlyEarned = 5,    YearlyEntitlement = 5,   RequiresEarnedDays = false } },
        { "MATERNITY",    new LeaveTypeConstraints { MaxDays = 90,   MonthlyEarned = 90,   YearlyEntitlement = 90,  RequiresEarnedDays = false } },
        { "PATERNITY",    new LeaveTypeConstraints { MaxDays = 14,   MonthlyEarned = 14,   YearlyEntitlement = 14,  RequiresEarnedDays = false } },
        { "SICK",         new LeaveTypeConstraints { MaxDays = 90,   MonthlyEarned = 90,   YearlyEntitlement = 90,  RequiresEarnedDays = false } },
        { "STUDY",        new LeaveTypeConstraints { MaxDays = null, MonthlyEarned = 0,    YearlyEntitlement = 0,   RequiresEarnedDays = false } },
        { "UNPAID",       new LeaveTypeConstraints { MaxDays = null, MonthlyEarned = 0,    YearlyEntitlement = 0,   RequiresEarnedDays = false } }
    };

    public LeaveApplicationViewModelValidator()
    {
        RuleFor(x => x.LeaveType ?? "ANNUAL")
            .Must(BeValidLeaveType)
            .WithMessage("Invalid leave type selected.");

        RuleFor(x => x)
            .Must(NotExceedLeaveTypeMaximum)
            .WithMessage(x => GetLeaveTypeMaximumMessage(x.LeaveType ?? "ANNUAL"));

        RuleFor(x => x)
            .Must(HaveValidHalfDayConfiguration)
            .WithMessage("Half‑day leave must be exactly 0.5 days with the same start and end date.")
            .When(x => x.HalfDay);

        RuleFor(x => x)
            .Must(HaveValidDateRange)
            .WithMessage("Invalid date range selected.");

        RuleFor(x => x)
            .Must(HaveReasonableDuration)
            .WithMessage(x => GetReasonableDurationMessage(x.LeaveType ?? "ANNUAL"));

        RuleFor(x => x.DaysApplied)
            .Must(BeInHalfDayIncrements)
            .WithMessage("Days applied must be in half‑day increments (0.5, 1, 1.5, …).");
    }

    private bool BeValidLeaveType(string leaveType) => !string.IsNullOrEmpty(leaveType) && _leaveTypeConstraints.ContainsKey(leaveType.ToUpperInvariant());
        
    private bool NotExceedLeaveTypeMaximum(LeaveApplicationViewModel vm)
    {
        if (string.IsNullOrEmpty(vm.LeaveType) ||
            !_leaveTypeConstraints.TryGetValue(vm.LeaveType.ToUpperInvariant(), out var c))
            return true; // let other validators handle unknown type

        return !(c.MaxDays.HasValue && vm.DaysApplied > c.MaxDays.Value);
    }

    private string GetLeaveTypeMaximumMessage(string leaveType)
    {
        if (string.IsNullOrEmpty(leaveType) ||
            !_leaveTypeConstraints.TryGetValue(leaveType.ToUpperInvariant(), out var c))
            return "Invalid leave type selected.";

        return c.MaxDays.HasValue
            ? $"You cannot apply for more than {c.MaxDays.Value} days of {leaveType} leave at a time."
            : "This leave type does not have a maximum day limit.";
    }

    private bool HaveValidHalfDayConfiguration(LeaveApplicationViewModel vm) => !vm.HalfDay || (vm.DaysApplied == 0.5m && vm.FromDate == vm.ToDate);
        
    private bool HaveValidDateRange(LeaveApplicationViewModel vm)
    {
        if (vm.FromDate == default || vm.ToDate == default) return false;
        var maxFuture = DateTime.Today.AddYears(1);
        return vm.FromDate <= vm.ToDate && vm.FromDate <= maxFuture && vm.ToDate <= maxFuture;
    }

    private bool HaveReasonableDuration(LeaveApplicationViewModel vm)
    {
        if (string.IsNullOrEmpty(vm.LeaveType) || !_leaveTypeConstraints.TryGetValue(vm.LeaveType.ToUpperInvariant(), out var leaveTypeConstraints))
            return true;

        if (vm.FromDate == null || vm.ToDate == null)
            return true;

        int consecutiveDays = (vm.ToDate.Value - vm.FromDate.Value).Days + 1;

        var maxDays = vm.LeaveType.ToUpperInvariant() switch
        {
            "SICK" => 90,
            "MATERNITY" => 90,
            "PATERNITY" => 14,
            "ADOPTION" => 5,
            "COMPASSIONATE" => 5,
            "ANNUAL" => 45, // ~30 business days
            "STUDY" => 15,
            "UNPAID" => 90,
            _ => 90
        };

        return consecutiveDays <= maxDays;
    }

    private string GetReasonableDurationMessage(string leaveType) =>
        $"The requested duration exceeds the maximum allowed for {leaveType} leave.";

    private bool BeInHalfDayIncrements(decimal days) =>
        days * 2 % 1 == 0;

    // Re‑used constraint class (unchanged)
    internal class LeaveTypeConstraints
    {
        public decimal? MaxDays { get; set; }
        public decimal MonthlyEarned { get; set; }
        public decimal YearlyEntitlement { get; set; }
        public bool RequiresEarnedDays { get; set; }
    }

}




