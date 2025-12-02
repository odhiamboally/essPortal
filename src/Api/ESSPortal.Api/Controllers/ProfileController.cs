using Asp.Versioning;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Profile;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESSPortal.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : BaseController
{
    private readonly IServiceManager _serviceManager;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IServiceManager serviceManager, ILogger<ProfileController> logger)
    {
        _serviceManager = serviceManager;
        _logger = logger;
    }

    // Read operations
    /// <summary>
    /// Gets user profile by user ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>User profile data</returns>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserProfile(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(ApiResponse<UserProfileResponse>.Failure("User ID is required."));
            }

            var result = await _serviceManager.ProfileService.GetUserProfileAsync(userId);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for user {UserId}", userId);
            return StatusCode(500, ApiResponse<UserProfileResponse>.Failure("An error occurred while retrieving user profile."));
        }
    }

    /// <summary>
    /// Validates profile data completeness and correctness
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate/{userId}")]
    public async Task<IActionResult> ValidateProfileData(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(ApiResponse<bool>.Failure("User ID is required."));
            }

            var result = await _serviceManager.ProfileService.ValidateProfileDataAsync(userId);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating profile data for user {UserId}", userId);
            return StatusCode(500, ApiResponse<bool>.Failure("An error occurred while validating profile data."));
        }
    }

    /// <summary>
    /// Calculates profile completion percentage
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Profile completion percentage</returns>
    [HttpGet("completion/{userId}")]
    public async Task<IActionResult> CalculateProfileCompletion(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(ApiResponse<int>.Failure("User ID is required."));
            }

            var result = await _serviceManager.ProfileService.CalculateProfileCompletionAsync(userId);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating profile completion for user {UserId}", userId);
            return StatusCode(500, ApiResponse<int>.Failure("An error occurred while calculating profile completion."));
        }
    }

    // Update operations
    /// <summary>
    /// Updates user personal details
    /// </summary>
    /// <param name="request">Personal details update request</param>
    /// <returns>Success or failure result</returns>
    [HttpPut("personal-details")]
    public async Task<IActionResult> UpdatePersonalDetails([FromBody] UpdatePersonalDetailsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.Failure("Invalid request data."));
            }

            var result = await _serviceManager.ProfileService.UpdatePersonalDetailsAsync(request);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal details for user {UserId}", request.UserId);
            return StatusCode(500, ApiResponse<bool>.Failure("An error occurred while updating personal details."));
        }
    }

    /// <summary>
    /// Updates user contact information
    /// </summary>
    /// <param name="request">Contact information update request</param>
    /// <returns>Success or failure result</returns>
    [HttpPut("contact-info")]
    public async Task<IActionResult> UpdateContactInfo([FromBody] UpdateContactInfoRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.Failure("Invalid request data."));
            }

            var result = await _serviceManager.ProfileService.UpdateContactInfoAsync(request);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact info for user {UserId}", request.UserId);
            return StatusCode(500, ApiResponse<bool>.Failure("An error occurred while updating contact information."));
        }
    }

    /// <summary>
    /// Updates user banking information
    /// </summary>
    /// <param name="request">Banking information update request</param>
    /// <returns>Success or failure result</returns>
    [HttpPut("banking-info")]
    public async Task<IActionResult> UpdateBankingInfo([FromBody] UpdateBankingInfoRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.Failure("Invalid request data."));
            }

            var result = await _serviceManager.ProfileService.UpdateBankingInfoAsync(request);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating banking info for user {UserId}", request.UserId);
            return StatusCode(500, ApiResponse<bool>.Failure("An error occurred while updating banking information."));
        }
    }

    // Create operations
    /// <summary>
    /// Updates user profile picture
    /// </summary>
    /// <param name="request">Profile picture update request</param>
    /// <returns>URL of the uploaded profile picture</returns>
    [HttpPost("profile-picture")]
    public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateProfilePictureRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Failure("Invalid request data."));
            }

            var result = await _serviceManager.ProfileService.UpdateProfilePictureAsync(request);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile picture for user {UserId}", request.UserId);
            return StatusCode(500, ApiResponse<string>.Failure("An error occurred while updating profile picture."));
        }
    }

    
}
