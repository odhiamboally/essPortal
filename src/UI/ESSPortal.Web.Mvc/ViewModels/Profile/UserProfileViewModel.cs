using ESSPortal.Web.Mvc.Models.Profile;

namespace ESSPortal.Web.Mvc.ViewModels.Profile;

public class UserProfileViewModel
{
    // Basic User Information
    public string UserId { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ProfilePictureFileName { get; set; }

    // Personal Details
    public PersonalDetailsViewModel PersonalDetails { get; set; } = new();

    // Contact Information
    public ContactInformationViewModel ContactInfo { get; set; } = new();

    // Banking Information
    public BankingInformationViewModel BankingInfo { get; set; } = new();

    // Employment Details
    public EmploymentDetailsViewModel EmploymentDetails { get; set; } = new();

    // Profile completion percentage
    public int ProfileCompletionPercentage => CalculateCompletionPercentage();

    // Recent activity
    public List<ProfileActivityItem> RecentActivity { get; set; } = [];

    public string GetProfilePictureFileName()
    {
        return string.IsNullOrWhiteSpace(ProfilePictureUrl) ? string.Empty : Path.GetFileName(ProfilePictureUrl);
    }

    private int CalculateCompletionPercentage()
    {
        var totalFields = 20; // Total important fields
        var completedFields = 0;

        // Check basic info
        if (!string.IsNullOrWhiteSpace(EmployeeNumber)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Email)) completedFields++;
        if (!string.IsNullOrWhiteSpace(PersonalDetails.FirstName)) completedFields++;
        if (!string.IsNullOrWhiteSpace(PersonalDetails.LastName)) completedFields++;
        if (!string.IsNullOrWhiteSpace(PersonalDetails.Gender)) completedFields++;

        // Check contact info
        if (!string.IsNullOrWhiteSpace(ContactInfo.MobileNo)) completedFields++;
        if (!string.IsNullOrWhiteSpace(ContactInfo.TelephoneNo)) completedFields++;
        if (!string.IsNullOrWhiteSpace(ContactInfo.PhysicalAddress)) completedFields++;
        if (!string.IsNullOrWhiteSpace(ContactInfo.City)) completedFields++;
        if (!string.IsNullOrWhiteSpace(ContactInfo.PostCode)) completedFields++;
        if (!string.IsNullOrWhiteSpace(ContactInfo.CountryRegionCode)) completedFields++;

        // Check banking info
        if (!string.IsNullOrWhiteSpace(BankingInfo.BankAccountNo)) completedFields++;
        if (!string.IsNullOrWhiteSpace(BankingInfo.KBABankCode)) completedFields++;
        if (!string.IsNullOrWhiteSpace(BankingInfo.KBABranchCode)) completedFields++;

        // Check employment details
        if (!string.IsNullOrWhiteSpace(EmploymentDetails.Department)) completedFields++;
        if (!string.IsNullOrWhiteSpace(EmploymentDetails.JobTitle)) completedFields++;
        if (!string.IsNullOrWhiteSpace(EmploymentDetails.ManagerId)) completedFields++;

        // Additional checks
        if (!string.IsNullOrWhiteSpace(ProfilePictureUrl)) completedFields++;
        if (!string.IsNullOrWhiteSpace(ContactInfo.ContactEMailAddress)) completedFields++;
        if (!string.IsNullOrWhiteSpace(ContactInfo.PostalAddress)) completedFields++;

        return (int)Math.Round((double)completedFields / totalFields * 100);
    }
}
