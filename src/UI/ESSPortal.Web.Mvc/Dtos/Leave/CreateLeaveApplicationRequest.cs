using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Mvc.Dtos.Leave;

public record CreateLeaveApplicationRequest
{
    public string ApplicationNo { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; init; } = DateTime.UtcNow;

    [Display(Name = "Apply on Behalf")]
    public bool ApplyOnBehalf { get; init; }

    [Required]
    [Display(Name = "Employee Number")]
    public string EmployeeNo { get; init; } = string.Empty;

    [Required]
    [Display(Name = "Employee Name")]
    public string EmployeeName { get; init; } = string.Empty;

    [Display(Name = "Mobile Number")]
    public string MobileNo { get; init; } = string.Empty;

    [Required]
    [Display(Name = "Leave Period")]
    public string LeavePeriod { get; init; } = string.Empty;

    [Required]
    [Display(Name = "Leave Type")]
    public string LeaveType { get; init; } = string.Empty;

    [Required]
    [Range(0.5, double.MaxValue, ErrorMessage = "Days applied must be at least 0.5")]
    [Display(Name = "Days Applied")]
    public decimal DaysApplied { get; init; }

    [Required]
    [Display(Name = "From Date")]
    public DateTime FromDate { get; init; }

    [Required]
    [Display(Name = "To Date")]
    public DateTime ToDate { get; init; }

    public DateTime ResumptionDate { get; init; }

    [Display(Name = "Leave Allowance Payable")]
    public bool LeaveAllowancePayable { get; init; }

    [Display(Name = "Selected Reliever")]
    public List<string> SelectedRelieverEmployeeNos { get; init; } = [];

    [Display(Name = "Attachments")]
    public IFormFile[]? Attachments { get; init; }

    [Required]
    [EmailAddress]
    public string EmailAddress { get; init; } = string.Empty;

    public string EmploymentType { get; init; } = string.Empty;
    public string ResponsibilityCenter { get; init; } = string.Empty;
    public string DutiesTakenOverBy { get; init; } = string.Empty;
    public string RelievingName { get; init; } = string.Empty;
    public bool HalfDay { get; init; }

    // Validation method
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(EmployeeNo))
            errors.Add("Employee number is required");

        if (string.IsNullOrWhiteSpace(EmployeeName))
            errors.Add("Employee name is required");

        if (string.IsNullOrWhiteSpace(LeaveType))
            errors.Add("Leave type is required");

        if (DaysApplied <= 0)
            errors.Add("Days applied must be greater than 0");

        if (FromDate >= ToDate)
            errors.Add("To date must be after from date");

        if (FromDate < DateTime.Today)
            errors.Add("From date cannot be in the past");

        if (SelectedRelieverEmployeeNos.Count == 0)
            errors.Add("Please select a reliever");

        if (SelectedRelieverEmployeeNos.Count > 1)
            errors.Add("Please select only one reliever");


        if (Attachments != null && Attachments.Length > 0)
        {
            string[] allowedTypes =
            [
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "text/plain",
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/gif",
                "application/zip",
                "application/x-rar-compressed"
            ];

            string[] allowedExtensions =
            [
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar"

            ];

            const long maxFileSize = 10 * 1024 * 1024; // 10MB

            foreach (var file in Attachments)
            {
                if (file.Length > maxFileSize)
                {
                    errors.Add($"File '{file.FileName}' exceeds maximum size of 10MB");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension) && !allowedTypes.Contains(file.ContentType))
                {
                    errors.Add($"File '{file.FileName}' has an unsupported file type");
                }
            }

            if (Attachments.Length > 10) // Reasonable limit
            {
                errors.Add("Maximum of 10 files can be uploaded");
            }
        }

        return errors.Count == 0;
    }



}
