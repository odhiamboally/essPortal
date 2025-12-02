using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ESSPortal.Domain.Entities;
public class AppUser : IdentityUser
{
    public string? EmployeeNumber { get; set; } 
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    [NotMapped]
    public string DisplayName => !string.IsNullOrWhiteSpace(FullName) ? FullName : UserName ?? Email ?? "Unknown User";

    public string? Gender { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? ManagerId { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTimeOffset? LastFailedLoginAt { get; set; }
    public DateTimeOffset? PasswordLastChanged { get; set; }
    public bool RequirePasswordChange { get; set; } = false;
    public string? TotpSecret { get; set; }
    public List<string>? BackupCodes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? DeactivatedAt { get; set; }



    // Navigation properties

    [JsonIgnore]
    public virtual AppUser? Manager { get; set; }

    [JsonIgnore]
    public virtual ICollection<AppUser> DirectReports { get; set; } = [];

    [JsonIgnore]
    public virtual ICollection<UserDevice> TrustedDevices { get; set; } = [];

    [JsonIgnore]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    [JsonIgnore]
    public virtual Profile? Profile { get; set; }



    public bool IsValidEmployee()
    {
        return !string.IsNullOrWhiteSpace(EmployeeNumber) &&
               !string.IsNullOrWhiteSpace(FirstName) &&
               !string.IsNullOrWhiteSpace(LastName) &&
               !string.IsNullOrWhiteSpace(Email) &&
               IsActive &&
               !IsDeleted;
    }

    // Methods
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LastFailedLoginAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementFailedLoginAttempts()
    {
        FailedLoginAttempts++;
        LastFailedLoginAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool ShouldRequirePasswordChange()
    {
        if (RequirePasswordChange) return true;

        // Check if password is older than 90 days (configurable)
        var passwordAge = DateTimeOffset.UtcNow - (PasswordLastChanged.HasValue ? PasswordLastChanged.Value : CreatedAt);
        return passwordAge.TotalDays > 90;
    }


}
