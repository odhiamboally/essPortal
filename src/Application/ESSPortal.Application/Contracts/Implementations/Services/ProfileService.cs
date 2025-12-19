using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Profile;
using ESSPortal.Domain.Entities;
using ESSPortal.Domain.Interfaces;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class ProfileService : IProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        ILogger<ProfileService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ApiResponse<UserProfileResponse>> GetUserProfileAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ApiResponse<UserProfileResponse>.Failure("User ID is required");
            }

            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return ApiResponse<UserProfileResponse>.Failure("User not found");
            }

            var userProfile = await _unitOfWork.UserProfileRepository.GetByUserIdAsync(userId);

            string? managerName = null;
            if (!string.IsNullOrWhiteSpace(appUser.ManagerId))
            {
                var manager = await _userManager.FindByIdAsync(appUser.ManagerId);
                managerName = manager != null ? $"{manager.FirstName} {manager.LastName}".Trim() : null;
            }

            var response = new UserProfileResponse(
                UserId: appUser.Id,
                EmployeeNumber: appUser.EmployeeNumber,
                FirstName: appUser.FirstName,
                MiddleName: appUser.MiddleName,
                LastName: appUser.LastName,
                Email: appUser.Email,
                PhoneNumber: appUser.PhoneNumber,
                Gender: appUser.Gender,
                ProfilePictureUrl: appUser.ProfilePictureUrl,
                Department: appUser.Department,
                JobTitle: appUser.JobTitle,
                ManagerId: appUser.ManagerId,
                ManagerName: managerName,
                LastLoginAt: appUser.LastLoginAt,
                CreatedAt: appUser.CreatedAt,
                IsActive: appUser.IsActive,
                No: string.Empty,
                CountryRegionCode: userProfile?.CountryRegionCode,
                PhysicalAddress: userProfile?.PhysicalAddress,
                TelephoneNo: userProfile?.TelephoneNo,
                MobileNo: userProfile?.MobileNo,
                PostalAddress: userProfile?.PostalAddress,
                PostCode: userProfile?.PostCode,
                City: userProfile?.City,
                ContactEMailAddress: userProfile?.ContactEMailAddress,

                BankAccountType: userProfile?.BankAccountType,
                KBABankCode: userProfile?.KBABankCode,
                KBABranchCode: userProfile?.KBABranchCode,
                BankAccountNo: userProfile?.BankAccountNo
            );

            _logger.LogInformation("Profile retrieved successfully for user: {UserId}", userId);
            return ApiResponse<UserProfileResponse>.Success("Profile retrieved successfully", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile for user: {UserId}", userId);
            return ApiResponse<UserProfileResponse>.Failure("An error occurred while retrieving the profile");
        }
    }

    public async Task<ApiResponse<bool>> UpdateBankingInfoAsync(UpdateBankingInfoRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return ApiResponse<bool>.Failure("User ID is required");
            }

            // Get existing profile to preserve contact info
            var existingProfile = await _unitOfWork.UserProfileRepository.GetByUserIdAsync(request.UserId);

            var profileToUpdate = new UserProfile
            {
                UserId = request.UserId,
                BankAccountType = request.BankAccountType,
                KBABankCode = request.KBABankCode,
                KBABranchCode = request.KBABranchCode,
                BankAccountNo = request.BankAccountNo,
                UpdatedBy = request.UserId,

                // Preserve existing contact info
                CountryRegionCode = existingProfile?.CountryRegionCode,
                PhysicalAddress = existingProfile?.PhysicalAddress,
                TelephoneNo = existingProfile?.TelephoneNo,
                MobileNo = existingProfile?.MobileNo,
                PostalAddress = existingProfile?.PostalAddress,
                PostCode = existingProfile?.PostCode,
                City = existingProfile?.City,
                ContactEMailAddress = existingProfile?.ContactEMailAddress
            };

            await _unitOfWork.UserProfileRepository.CreateOrUpdateAsync(request.UserId, profileToUpdate);

            _logger.LogInformation("Banking information updated successfully for user: {UserId}", request.UserId);
            return ApiResponse<bool>.Success("Banking information updated successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating banking information for user: {UserId}", request.UserId);
            return ApiResponse<bool>.Failure("An error occurred while updating banking information");
        }
    }

    public async Task<ApiResponse<bool>> UpdateContactInfoAsync(UpdateContactInfoRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return ApiResponse<bool>.Failure("User ID is required");
            }

            var appUser = await _userManager.FindByIdAsync(request.UserId);
            if (appUser == null)
            {
                _logger.LogWarning("User not found for contact info update: {UserId}", request.UserId);
                return ApiResponse<bool>.Failure("User not found");
            }

            appUser.PhoneNumber = request.MobileNo;
            appUser.UpdatedAt = DateTimeOffset.UtcNow;
            appUser.UpdatedBy = request.UserId;

            var userResult = await _userManager.UpdateAsync(appUser);
            if (!userResult.Succeeded)
            {
                _logger.LogError("Failed to update AppUser contact info: {Errors}",
                    string.Join(", ", userResult.Errors.Select(e => e.Description)));
                return ApiResponse<bool>.Failure("Failed to update contact information");
            }

            var existingProfile = await _unitOfWork.UserProfileRepository.GetByUserIdAsync(request.UserId);

            var profileToUpdate = new UserProfile
            {
                UserId = request.UserId,
                CountryRegionCode = request.CountryRegionCode,
                PhysicalAddress = request.PhysicalAddress,
                TelephoneNo = request.TelephoneNo,
                MobileNo = request.MobileNo,
                PostalAddress = request.PostalAddress,
                PostCode = request.PostCode,
                City = request.City,
                ContactEMailAddress = request.ContactEMailAddress,
                UpdatedBy = request.UserId,

                // Preserve existing banking info if updating existing profile
                BankAccountType = existingProfile?.BankAccountType,
                KBABankCode = existingProfile?.KBABankCode,
                KBABranchCode = existingProfile?.KBABranchCode,
                BankAccountNo = existingProfile?.BankAccountNo
            };

            await _unitOfWork.UserProfileRepository.CreateOrUpdateAsync(request.UserId, profileToUpdate);
            
            _logger.LogInformation("Contact information updated successfully for user: {UserId}", request.UserId);
            return ApiResponse<bool>.Success("Contact information updated successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact information for user: {UserId}", request.UserId);
            return ApiResponse<bool>.Failure("An error occurred while updating contact information");
        }
    }

    public async Task<ApiResponse<bool>> UpdatePersonalDetailsAsync(UpdatePersonalDetailsRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return ApiResponse<bool>.Failure("User ID is required");
            }

            var appUser = await _userManager.FindByIdAsync(request.UserId);
            if (appUser == null)
            {
                _logger.LogWarning("User not found for personal details update: {UserId}", request.UserId);
                return ApiResponse<bool>.Failure("User not found");
            }

            appUser.FirstName = request.FirstName;
            appUser.MiddleName = request.MiddleName;
            appUser.LastName = request.LastName;
            appUser.Gender = request.Gender;
            appUser.UpdatedAt = DateTimeOffset.UtcNow;
            appUser.UpdatedBy = request.UserId;

            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update AppUser personal details: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<bool>.Failure("Failed to update personal details");
            }

            _logger.LogInformation("Personal details updated successfully for user: {UserId}", request.UserId);
            return ApiResponse<bool>.Success("Personal details updated successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal details for user: {UserId}", request.UserId);
            return ApiResponse<bool>.Failure("An error occurred while updating personal details");
        }
    }

    public async Task<ApiResponse<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return ApiResponse<string>.Failure("User ID is required");
            }

            var appUser = await _userManager.FindByIdAsync(request.UserId);
            if (appUser == null)
            {
                _logger.LogWarning("User not found for profile picture update: {UserId}", request.UserId);
                return ApiResponse<string>.Failure("User not found");
            }

            // Simple file processing - just generate URL since file is handled by frontend FileService
            var fileExtension = Path.GetExtension(request.FileName).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                fileExtension = ".jpg"; // Default extension
            }

            if (!fileExtension.StartsWith("."))
            {
                fileExtension = $".{fileExtension}"; // Ensure it starts with dot
            }

            // Create clean username (remove special characters, spaces, etc.)
            var cleanUserName = CleanFileName(appUser.UserName ?? appUser.Email?.Split('@')[0] ?? "user");

            var shortGuid = Guid.NewGuid().ToString("N")[..8]; // First 8 characters
            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{cleanUserName}_{timestamp}_{shortGuid}{fileExtension}";

            var profilePictureUrl = $"/Images/ProfilePictures/{fileName}";

            appUser.ProfilePictureUrl = profilePictureUrl;
            appUser.UpdatedAt = DateTimeOffset.UtcNow;
            appUser.UpdatedBy = request.UserId;

            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update profile picture URL: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure("Failed to update profile picture");
            }

            _logger.LogInformation("Profile picture updated successfully for user: {UserId}", request.UserId);

            // Return both URL and filename so frontend can save the file with matching name
            return ApiResponse<string>.Success("Profile picture updated successfully", $"{profilePictureUrl}|{fileName}"); // Frontend can split this
                
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile picture for user: {UserId}", request.UserId);
            return ApiResponse<string>.Failure("An error occurred while updating profile picture");
        }
    }

    public async Task<ApiResponse<bool>> ValidateProfileDataAsync(string userId)
    {
        try
        {
            var profileResponse = await GetUserProfileAsync(userId);
            if (!profileResponse.Successful || profileResponse.Data == null)
            {
                return ApiResponse<bool>.Failure("Unable to validate profile - profile not found");
            }

            var profile = profileResponse.Data;
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(profile.FirstName))
                validationErrors.Add("First name is required");

            if (string.IsNullOrWhiteSpace(profile.LastName))
                validationErrors.Add("Last name is required");

            // Validate contact information
            if (string.IsNullOrWhiteSpace(profile.PhoneNumber) && string.IsNullOrWhiteSpace(profile.MobileNo))
                validationErrors.Add("At least one phone number is required");

            if (!string.IsNullOrWhiteSpace(profile.Email) && !IsValidEmail(profile.Email))
                validationErrors.Add("Invalid email format");

            // Validate banking information (if provided)
            if (!string.IsNullOrWhiteSpace(profile.BankAccountNo))
            {
                if (string.IsNullOrWhiteSpace(profile.KBABankCode))
                    validationErrors.Add("Bank code is required when account number is provided");

                if (string.IsNullOrWhiteSpace(profile.KBABranchCode))
                    validationErrors.Add("Branch code is required when account number is provided");
            }

            if (validationErrors.Any())
            {
                return ApiResponse<bool>.Failure($"Validation errors: {string.Join(", ", validationErrors)}");
            }

            return ApiResponse<bool>.Success("Profile validation passed", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating profile for user {UserId}", userId);
            return ApiResponse<bool>.Failure("An error occurred during profile validation");
        }
    }

    public async Task<ApiResponse<int>> CalculateProfileCompletionAsync(string userId)
    {
        try
        {
            var profileResponse = await GetUserProfileAsync(userId);
            if (!profileResponse.Successful || profileResponse.Data == null)
            {
                return ApiResponse<int>.Failure("Unable to calculate completion - profile not found");
            }

            var profile = profileResponse.Data;
            var totalFields = 20; // Total important fields
            var completedFields = 0;

            // Check basic info (5 fields)
            if (!string.IsNullOrWhiteSpace(profile.EmployeeNumber)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.Email)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.FirstName)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.LastName)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.Gender)) completedFields++;

            // Check contact info (6 fields)
            if (!string.IsNullOrWhiteSpace(profile.MobileNo)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.TelephoneNo)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.PhysicalAddress)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.City)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.PostCode)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.CountryRegionCode)) completedFields++;

            // Check banking info (3 fields)
            if (!string.IsNullOrWhiteSpace(profile.BankAccountNo)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.KBABankCode)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.KBABranchCode)) completedFields++;

            // Check employment details (3 fields)
            if (!string.IsNullOrWhiteSpace(profile.Department)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.JobTitle)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.ManagerId)) completedFields++;

            // Additional checks (3 fields)
            if (!string.IsNullOrWhiteSpace(profile.ProfilePictureUrl)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.ContactEMailAddress)) completedFields++;
            if (!string.IsNullOrWhiteSpace(profile.PostalAddress)) completedFields++;

            var percentage = (int)Math.Round((double)completedFields / totalFields * 100);

            _logger.LogDebug("Profile completion calculated for user {UserId}: {Percentage}% ({CompletedFields}/{TotalFields})",
                userId, percentage, completedFields, totalFields);

            return ApiResponse<int>.Success("Profile completion calculated", percentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating profile completion for user {UserId}", userId);
            return ApiResponse<int>.Failure("An error occurred while calculating profile completion");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static string CleanFileName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "user";

        // Remove invalid filename characters and spaces
        var invalidChars = Path.GetInvalidFileNameChars().Concat([' ', '@', '.', '-']).ToArray();
        var cleaned = string.Join("", input.Where(c => !invalidChars.Contains(c)));

        // Limit length and ensure it's not empty
        cleaned = cleaned.Length > 15 ? cleaned[..15] : cleaned;
        return string.IsNullOrWhiteSpace(cleaned) ? "user" : cleaned.ToLowerInvariant();
    }



}
