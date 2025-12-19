using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Controllers;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Dtos.Profile;
using ESSPortal.Web.Mvc.Extensions;
using ESSPortal.Web.Mvc.Mappings;
using ESSPortal.Web.Mvc.Utilities.Session;
using ESSPortal.Web.Mvc.Validations.RequestValidators.Profile;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace ESSPortal.Web.Mvc.Controllers;


[Authorize]
public class ProfileController : BaseController
{
    

    public ProfileController(
        IServiceManager serviceManager, 
        IOptions<AppSettings> appSettings, 
        ILogger<AuthController> logger 
        

        ): base(serviceManager, appSettings, logger) { }
       

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = CurrentUserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                this.ToastError("User session expired. Please log in again.", "Session Expired");
                return RedirectToAction("SignIn", "Auth");
            }

            var profileResponse = await _serviceManager.ProfileService.GetUserProfileAsync(userId);
            if (!profileResponse.Successful || profileResponse.Data == null)
            {
                this.ToastError("Failed to load profile data.", "Profile Error");
                return RedirectToAction("Index", "Dashboard");
            }

            var profile = profileResponse.Data;

            var viewModel = profile.ToUserProfileViewModel();

            var userInfo = GetUserInfoFromSession();
            if (userInfo == null)
            {
                userInfo = CacheServiceExtensions.GetUserInfo(_serviceManager.CacheService, _currentUser?.EmployeeNumber ?? string.Empty);
            }

            viewModel.EmploymentDetails.ManagerName = userInfo?.ManagerSupervisor ?? "N/A";
            viewModel.EmploymentDetails.Department = userInfo?.ResponsibilityCenter ?? "N/A";
            viewModel.EmploymentDetails.JobTitle = userInfo?.JobPositionTitle ?? "N/A";

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user profile for user {UserId}", CurrentUserId);
            this.ToastError("An error occurred while loading your profile.", "Profile Error");
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePersonalDetails(UpdatePersonalDetailsRequest request)
    {
        try
        {
            var validator = new UpdatePersonalDetailsRequestValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                this.ToastError(errors, "Validation Error");
                return RedirectToAction("Index");
            }

            var userId = CurrentUserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                this.ToastError("User session expired. Please log in again.", "Session Expired");
                return RedirectToAction("SignIn", "Auth");
            }

            request.UserId = userId;

            var response = await _serviceManager.ProfileService.UpdatePersonalDetailsAsync(request);

            if (response.Successful)
            {
                this.ToastSuccess("Your personal details have been updated successfully.", "Update Successful");
                _logger.LogInformation("Personal details updated for user {UserId}", userId);
            }
            else
            {
                this.ToastError(response.Message ?? "Failed to update personal details. Please try again.", "Update Failed");
                _logger.LogWarning("Failed to update personal details for user {UserId}: {Error}", userId, response.Message);
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal details for user {UserId}", CurrentUserId);
            this.ToastError("An error occurred while updating your personal details.", "Update Error");
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateContactInfo(UpdateContactInfoRequest request)
    {
        try
        {
            var validator = new UpdateContactInfoRequestValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                this.ToastError(errors, "Validation Error");
                return RedirectToAction("Index");
            }

            var userId = CurrentUserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                this.ToastError("User session expired. Please log in again.", "Session Expired");
                return RedirectToAction("SignIn", "Auth");
            }

            request.UserId = userId;

            var response = await _serviceManager.ProfileService.UpdateContactInfoAsync(request);

            if (response.Successful)
            {
                this.ToastSuccess("Your contact information has been updated successfully.", "Update Successful");
                _logger.LogInformation("Contact information updated for user {UserId}", userId);
            }
            else
            {
                this.ToastError(response.Message ?? "Failed to update contact information. Please try again.", "Update Failed");
                _logger.LogWarning("Failed to update contact info for user {UserId}: {Error}", userId, response.Message);
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact information for user {UserId}", CurrentUserId);
            this.ToastError("An error occurred while updating your contact information.", "Update Error");
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBankingInfo(UpdateBankingInfoRequest request)
    {
        try
        {
            var validator = new UpdateBankingInfoRequestValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                this.ToastError(errors, "Validation Error");
                return RedirectToAction("Index");
            }

            var userId = CurrentUserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                this.ToastError("User session expired. Please log in again.", "Session Expired");
                return RedirectToAction("SignIn", "Auth");
            }

            request.UserId = userId;

            var response = await _serviceManager.ProfileService.UpdateBankingInfoAsync(request);

            if (response.Successful)
            {
                this.ToastSuccess("Your banking information has been updated successfully.", "Update Successful");
                _logger.LogInformation("Banking information updated for user {UserId}", userId);
            }
            else
            {
                this.ToastError(response.Message ?? "Failed to update banking information. Please try again.", "Update Failed");
                _logger.LogWarning("Failed to update banking info for user {UserId}: {Error}", userId, response.Message);
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating banking information for user {UserId}", CurrentUserId);
            this.ToastError("An error occurred while updating your banking information.", "Update Error");
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfilePicture(UpdateProfilePictureRequest request)
    {
        try
        {
            var validator = new UpdateProfilePictureRequestValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                this.ToastError(errors, "Validation Error");
                return RedirectToAction("Index");
            }

            var userId = CurrentUserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                this.ToastError("User session expired. Please log in again.", "Session Expired");
                return RedirectToAction("SignIn", "Auth");
            }

            // Convert file to bytes and base64
            if (request.ProfilePicture == null || request.ProfilePicture.Length == 0)
            {
                this.ToastError("Please select a valid profile picture.", "Profile Picture Error");
                return RedirectToAction("Index");
            }

            var imageBytes = await request.ProfilePicture.ToBytesAsync();
            var base64Content = Convert.ToBase64String(imageBytes);

            request.UserId = userId;
            //request.FileName = pictureFileName ?? string.Empty;
            request.ContentType = request.ProfilePicture.ContentType ?? "image/jpeg";
            request.Base64Content = base64Content;

            var response = await _serviceManager.ProfileService.UpdateProfilePictureAsync(request);

            if (response.Successful)
            {
                // Parse response to get URL and filename
                var responseParts = response.Data?.Split('|');
                if (responseParts?.Length == 2)
                {
                    var fileName = responseParts[1];

                    // Save file using FileService
                    var saveResult = await _serviceManager.FileService.SaveProfilePictureAsync(userId, imageBytes, fileName);

                    if (saveResult.Successful)
                    {
                        this.ToastSuccess("Your profile picture has been updated successfully.", "Update Successful");
                        _logger.LogInformation("Profile picture updated for user {UserId} with filename {FileName}",
                        userId, fileName);
                    }
                    else
                    {
                        this.ToastError("Failed to save profile picture file. Please try again.", "File Save Error");
                        _logger.LogError("Failed to save profile picture file for user {UserId}: {Error}",
                            userId, saveResult.Message);
                    }
                }
                else
                {
                    this.ToastSuccess("Your profile picture has been updated successfully.", "Update Successful");
                    _logger.LogInformation("Profile picture updated for user {UserId}", userId);
                }
            }
            else
            {
                this.ToastError(response.Message ?? "Failed to update profile picture. Please try again.", "Update Failed");
                _logger.LogWarning("Failed to update profile picture for user {UserId}: {Error}", userId, response.Message);
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile picture for user {UserId}", CurrentUserId);
            this.ToastError("An error occurred while updating your profile picture.", "Update Error");
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    [Route("Profile/Image/{fileName}")]
    public async Task<IActionResult> GetProfileImage(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return NotFound();
        }

        try
        {
            var (imageBytes, contentType) = await _serviceManager.FileService.ReadProfilePictureAsync(fileName);

            if (imageBytes.Length == 0)
            {
                // Return a default avatar image or 404
                return NotFound();
            }

            // Set caching headers for better performance
            Response.Headers.Append("Cache-Control", "public, max-age=31536000"); // 1 year
            Response.Headers.Append("Expires", DateTimeOffset.UtcNow.AddYears(1).ToString("R"));

            return File(imageBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving profile image: {FileName}", fileName);
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetProfileCompletion()
    {
        try
        {
            var userId = CurrentUserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var profileResponse = await _serviceManager.ProfileService.GetUserProfileAsync(userId);
            if (!profileResponse.Successful || profileResponse.Data == null)
            {
                return Json(new { success = false, message = "Failed to load profile" });
            }

            var viewModel = profileResponse.Data.ToUserProfileViewModel();
            return Json(new
            {
                success = true,
                completionPercentage = viewModel.ProfileCompletionPercentage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile completion for user {UserId}", CurrentUserId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    


}
