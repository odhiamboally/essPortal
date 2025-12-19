using Azure.Core;

using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Profile;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using ESSPortal.Web.Blazor.Dtos.Profile;
using ESSPortal.Web.Blazor.Utilities.Api;
using ESSPortal.Web.Blazor.ViewModels.Profile;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.AppServices;

public class ProfileService : IProfileService
{
    private readonly ILogger<ProfileService> _logger;
    private readonly IServiceManager _serviceManager;

    public ProfileService(ILogger<ProfileService> logger, IServiceManager serviceManager)
    {
        _logger = logger;
        _serviceManager = serviceManager;
    }

    public async Task<ApiResponse<UserProfileResponse>> GetUserProfileAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ApiResponse<UserProfileResponse>.Failure("User ID is required");
            }

            var response = await _serviceManager.ProfileService.GetUserProfileAsync(userId);

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to get user profile for user {UserId}: {Error}", userId, response.Message);
                return ApiResponse<UserProfileResponse>.Failure(response.Message ?? "Failed to retrieve profile");
            }

            return ApiResponse<UserProfileResponse>.Success("Profile retrieved successfully", response.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for user {UserId}", userId);
            throw;
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

            var response = await _serviceManager.ProfileService.UpdatePersonalDetailsAsync(request);

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to update personal details for user {UserId}: {Error}",request.UserId, response.Message);
                    
                return ApiResponse<bool>.Failure(response.Message ?? "Failed to update personal details");
            }

            return ApiResponse<bool>.Success("Personal details updated successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal details for user {UserId}", request.UserId);
            throw;
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

            var response = await _serviceManager.ProfileService.UpdateContactInfoAsync(request);

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to update contact info for user {UserId}: {Error}",
                    request.UserId, response.Message);
                return ApiResponse<bool>.Failure(response.Message ?? "Failed to update contact information");
            }

            _logger.LogInformation("Contact information updated successfully for user {UserId}", request.UserId);
            return ApiResponse<bool>.Success("Contact information updated successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact info for user {UserId}", request.UserId);
            throw;
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

            var response = await _serviceManager.ProfileService.UpdateBankingInfoAsync(request);

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to update banking info for user {UserId}: {Error}",
                    request.UserId, response.Message);
                return ApiResponse<bool>.Failure(response.Message ?? "Failed to update banking information");
            }

            _logger.LogInformation("Banking information updated successfully for user {UserId}", request.UserId);
            return ApiResponse<bool>.Success("Banking information updated successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating banking info for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<ApiResponse<string>> UpdateProfilePictureAsync(UpdateProfilePictureViewModel viewModel)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(viewModel.UserId))
            {
                return ApiResponse<string>.Failure("User ID is required");
            }

            var request = new UpdateProfilePictureRequest(viewModel.UserId, viewModel.FileName, viewModel.ContentType, viewModel.Base64Content);

            var response = await _serviceManager.ProfileService.UpdateProfilePictureAsync(request);
            if (!response.Successful) 
            {
                _logger.LogWarning("Failed to update profile picture for user {UserId}: {Error}", request.UserId, response.Message);
                    
                return ApiResponse<string>.Failure(response.Message ?? "Failed to update profile picture");
            }

            return ApiResponse<string>.Success("Profile picture updated successfully", response.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile picture for user {UserId}", viewModel.UserId);

            throw;
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

            // Validate required personal details
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
            throw;
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
            throw;
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            throw;
        }
    }

    private static string GetIconForAction(string action)
    {
        return action.ToLowerInvariant() switch
        {
            "profile updated" => "📝",
            "password changed" => "🔑",
            "profile picture updated" => "📷",
            "contact updated" => "📞",
            "banking updated" => "🏦",
            "personal details updated" => "👤",
            _ => "📋"
        };
    }

    

  




}

