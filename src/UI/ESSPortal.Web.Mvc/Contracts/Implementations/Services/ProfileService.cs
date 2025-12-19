using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Dtos.Profile;
using ESSPortal.Web.Mvc.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.AppServices;

public class ProfileService : IProfileService
{
    private readonly IApiService _apiService;
    private readonly ILogger<ProfileService> _logger;
    private readonly ApiSettings _apiSettings;

    public ProfileService(IApiService apiService, ILogger<ProfileService> logger, IOptions<ApiSettings> apiSettings)
    {
        _apiService = apiService;
        _logger = logger;
        _apiSettings = apiSettings.Value;
    }

    public async Task<AppResponse<UserProfileResponse>> GetUserProfileAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return AppResponse<UserProfileResponse>.Failure("User ID is required");
            }

            var endpoint = _apiSettings.ApiEndpoints?.Profile?.GetUserProfile;
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return AppResponse<UserProfileResponse>.Failure("Endpoint is not configured.");
            }

            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
            endpoint = EndpointHelper.ReplaceParams(endpoint, new Dictionary<string, string>
            {
                { "userId", userId }
            });

            var response = await _apiService.GetAsync<UserProfileResponse>(endpoint);

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to get user profile for user {UserId}: {Error}", userId, response.Message);
                return AppResponse<UserProfileResponse>.Failure(response.Message ?? "Failed to retrieve profile");
            }

            return AppResponse<UserProfileResponse>.Success("Profile retrieved successfully", response.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for user {UserId}", userId);
            return AppResponse<UserProfileResponse>.Failure("An error occurred while retrieving the profile");
        }
    }

    public async Task<AppResponse<bool>> UpdatePersonalDetailsAsync(UpdatePersonalDetailsRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return AppResponse<bool>.Failure("User ID is required");
            }

            var endpoint = _apiSettings.ApiEndpoints?.Profile?.UpdatePersonalDetails;
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return AppResponse<bool>.Failure("Endpoint is not configured.");
            }

            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
            
            var response = await _apiService.PutAsync<UpdatePersonalDetailsRequest, bool>(endpoint, request);

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to update personal details for user {UserId}: {Error}",request.UserId, response.Message);
                    
                return AppResponse<bool>.Failure(response.Message ?? "Failed to update personal details");
            }

            _logger.LogInformation("Personal details updated successfully for user {UserId}", request.UserId);
            return AppResponse<bool>.Success("Personal details updated successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal details for user {UserId}", request.UserId);
            return AppResponse<bool>.Failure("An error occurred while updating personal details");
        }
    }

    public async Task<AppResponse<bool>> UpdateContactInfoAsync(UpdateContactInfoRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return AppResponse<bool>.Failure("User ID is required");
            }

            var endpoint = _apiSettings.ApiEndpoints?.Profile?.UpdateContactInfo;
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return AppResponse<bool>.Failure("Endpoint is not configured.");
            }

            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var response = await _apiService.PutAsync<UpdateContactInfoRequest, bool>(endpoint, request);

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to update contact info for user {UserId}: {Error}",
                    request.UserId, response.Message);
                return AppResponse<bool>.Failure(response.Message ?? "Failed to update contact information");
            }

            _logger.LogInformation("Contact information updated successfully for user {UserId}", request.UserId);
            return AppResponse<bool>.Success("Contact information updated successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact info for user {UserId}", request.UserId);
            return AppResponse<bool>.Failure("An error occurred while updating contact information");
        }
    }

    public async Task<AppResponse<bool>> UpdateBankingInfoAsync(UpdateBankingInfoRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return AppResponse<bool>.Failure("User ID is required");
            }

            var endpoint = _apiSettings.ApiEndpoints?.Profile?.UpdateBankingInfo;
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return AppResponse<bool>.Failure("Endpoint is not configured.");
            }

            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var response = await _apiService.PutAsync<UpdateBankingInfoRequest, bool>(endpoint, request);

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to update banking info for user {UserId}: {Error}",
                    request.UserId, response.Message);
                return AppResponse<bool>.Failure(response.Message ?? "Failed to update banking information");
            }

            _logger.LogInformation("Banking information updated successfully for user {UserId}", request.UserId);
            return AppResponse<bool>.Success("Banking information updated successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating banking info for user {UserId}", request.UserId);
            return AppResponse<bool>.Failure("An error occurred while updating banking information");
        }
    }

    public async Task<AppResponse<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return AppResponse<string>.Failure("User ID is required");
            }

            var endpoint = _apiSettings.ApiEndpoints?.Profile?.UpdateProfilePicture;
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return AppResponse<string>.Failure("Endpoint is not configured.");
            }

            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var response = await _apiService.PostAsync<UpdateProfilePictureRequest, string>(endpoint, request);

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to update profile picture for user {UserId}: {Error}",
                    request.UserId, response.Message);
                return AppResponse<string>.Failure(response.Message ?? "Failed to update profile picture");
            }

            _logger.LogInformation("Profile picture updated successfully for user {UserId}", request.UserId);
            return AppResponse<string>.Success("Profile picture updated successfully", response.Data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile picture for user {UserId}", request.UserId);
            return AppResponse<string>.Failure("An error occurred while updating profile picture");
        }
    }

    public async Task<AppResponse<bool>> ValidateProfileDataAsync(string userId)
    {
        try
        {
            var profileResponse = await GetUserProfileAsync(userId);
            if (!profileResponse.Successful || profileResponse.Data == null)
            {
                return AppResponse<bool>.Failure("Unable to validate profile - profile not found");
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
                return AppResponse<bool>.Failure($"Validation errors: {string.Join(", ", validationErrors)}");
            }

            return AppResponse<bool>.Success("Profile validation passed", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating profile for user {UserId}", userId);
            return AppResponse<bool>.Failure("An error occurred during profile validation");
        }
    }

    public async Task<AppResponse<int>> CalculateProfileCompletionAsync(string userId)
    {
        try
        {
            var profileResponse = await GetUserProfileAsync(userId);
            if (!profileResponse.Successful || profileResponse.Data == null)
            {
                return AppResponse<int>.Failure("Unable to calculate completion - profile not found");
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

            return AppResponse<int>.Success("Profile completion calculated", percentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating profile completion for user {UserId}", userId);
            return AppResponse<int>.Failure("An error occurred while calculating profile completion");
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
            return false;
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

