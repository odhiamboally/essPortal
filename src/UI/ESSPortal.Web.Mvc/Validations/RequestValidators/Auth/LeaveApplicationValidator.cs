using ESSPortal.Web.Mvc.ViewModels.Leave;

using FluentValidation;

namespace ESSPortal.Web.Mvc.Validations.RequestValidators.Auth;

public class LeaveApplicationValidator : AbstractValidator<LeaveApplicationViewModel>
{
    public LeaveApplicationValidator()
    {
        // Employee Details Validation
        RuleFor(x => x.EmployeeNo)
            .NotEmpty()
            .WithMessage("Employee number is required.");

        RuleFor(x => x.EmployeeName)
            .NotEmpty()
            .WithMessage("Employee name is required.")
            .MaximumLength(100)
            .WithMessage("Employee name cannot exceed 100 characters.");

        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .WithMessage("Email address is required.")
            .EmailAddress()
            .WithMessage("Please enter a valid email address.");

        RuleFor(x => x.MobileNo)
            .NotEmpty()
            .WithMessage("Mobile number is required.")
            .Matches(@"^[\+]?[0-9\s\-\(\)]+$")
            .WithMessage("Please enter a valid mobile number.");

        // Leave Details Validation
        RuleFor(x => x.LeavePeriod)
            .NotEmpty()
            .WithMessage("Please select a leave period.");

        RuleFor(x => x.LeaveType)
            .NotEmpty()
            .WithMessage("Please select a leave type.");

        RuleFor(x => x.DaysApplied)
            .GreaterThan(0)
            .WithMessage("Days applied must be greater than 0.")
            .LessThanOrEqualTo(365)
            .WithMessage("Days applied cannot exceed 365 days.");

        RuleFor(x => x.FromDate)
            .NotNull()
            .WithMessage("From date is required.")
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("From date cannot be in the past.");

        RuleFor(x => x.ToDate)
            .NotNull()
            .WithMessage("To date is required.")
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("To date must be after or equal to from date.");

        RuleFor(x => x.ResumptionDate)
            .NotNull()
            .WithMessage("Resumption date is required.")
            .GreaterThan(x => x.ToDate)
            .WithMessage("Resumption date must be after the to date.");

        // Business Rule: Cannot apply for more days than earned
        RuleFor(x => x.DaysApplied)
            .LessThanOrEqualTo(x => x.LeaveEarnedToDate)
            .WithMessage("Cannot apply for more days than earned to date.")
            .When(x => x.LeaveEarnedToDate > 0);

        // Date Range Validation
        RuleFor(x => x)
            .Must(HaveValidDateRange)
            .WithMessage("The date range is invalid. Please check your dates.")
            .WithName("DateRange");

        // File Upload Validation
        RuleForEach(x => x.Attachments)
            .Must(BeValidFileSize)
            .WithMessage("File size must not exceed 10MB.")
            .Must(BeValidFileType)
            .WithMessage("Invalid file type. Only documents, images, and common file types are allowed.");

        // Optional: Responsibility Center validation
        RuleFor(x => x.ResponsibilityCenter)
            .NotEmpty()
            .WithMessage("Responsibility center is required.")
            .When(x => !string.IsNullOrWhiteSpace(x.EmploymentType));
    }

    private bool HaveValidDateRange(LeaveApplicationViewModel model)
    {
        if (!model.FromDate.HasValue || !model.ToDate.HasValue)
            return false;

        var businessDays = CalculateBusinessDays(model.FromDate.Value, model.ToDate.Value);
        return businessDays == (int)model.DaysApplied;
    }

    private int CalculateBusinessDays(DateTime startDate, DateTime endDate)
    {
        int businessDays = 0;
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

    private bool BeValidFileSize(IFormFile file)
    {
        if (file == null) return true;
        const long maxSizeInBytes = 10 * 1024 * 1024; // 10MB
        return file.Length <= maxSizeInBytes;
    }

    private bool BeValidFileType(IFormFile file)
    {
        if (file == null) return true;

        string[] allowedExtensions =
        [
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".txt", ".rtf", ".jpg", ".jpeg", ".png", ".gif", ".bmp",
            ".zip", ".rar", ".7z"
        ];

        var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        return !string.IsNullOrWhiteSpace(fileExtension) && allowedExtensions.Contains(fileExtension);
    }
}
