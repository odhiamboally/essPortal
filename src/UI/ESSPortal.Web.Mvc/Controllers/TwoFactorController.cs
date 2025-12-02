using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Controllers;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Dtos.TwoFactor;
using ESSPortal.Web.Mvc.Extensions;
using ESSPortal.Web.Mvc.ViewModels.TwoFactor;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Controllers;

[Authorize]
public class TwoFactorController : BaseController
{

    public TwoFactorController(
        IServiceManager serviceManager,
        IOptions<AppSettings> appSettings,
        ILogger<TwoFactorController> logger
        ): base(serviceManager, appSettings, logger) { }
        

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        try
        {
            var result = await _serviceManager.TwoFactorService.GetTwoFactorStatusAsync();
            if (!result.Successful)
            {
                this.ToastError(result.Message ?? "Failed to retrieve two-factor authentication status.");
                return RedirectToAction("Index", "Profile");
            }

            if (result.Data == null)
            {
                this.ToastError("Two-factor authentication status is not available.");
                return RedirectToAction("Index", "Profile"); 
            }

            if (result.Data.IsEnabled && !result.Data.HasBackupCodes)
            {
                this.ToastWarning("You have two-factor authentication enabled but no backup codes generated. Please generate backup codes for account recovery.");
                return RedirectToAction("Index", "Profile");
            }

            if (result.Data.IsEnabled && !result.Data.HasBackupCodes)
            {
                this.ToastWarning("Two-factor authentication is enabled, but you have not generated backup codes. Please generate backup codes for account recovery.");
            } 
            
            else 
            {
                this.ToastInfo("Two-factor authentication is not enabled. You can enable it for added security.");
            }

            var model = new TwoFactorSettingsViewModel
            {
                IsTwoFactorEnabled = result.Data?.IsEnabled ?? false,
                HasBackupCodes = result.Data?.HasBackupCodes ?? false,
                CanEnable2FA = !result.Data?.IsEnabled ?? true
            };

            return PartialView("_TwoFactorSettingsModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading 2FA settings");
            this.ToastError("An error occurred while loading two-factor authentication settings.");
            return PartialView("_TwoFactorSettingsModal", new TwoFactorSettingsViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Setup()
    {
        try
        {
            var result = await _serviceManager.TwoFactorService.GetSetupInfoAsync();

            if (!result.Successful)
            {
                this.ToastError(result.Message ?? "Failed to retrieve setup information.");
                return RedirectToAction("Index", "Profile");
            }

            var model = new TwoFactorSetupViewModel
            {
                QrCodeUri = result.Data?.QrCodeUri ?? string.Empty,
                ManualEntryKey = result.Data?.ManualEntryKey ?? string.Empty,
                IsInitialSetup = true 
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up 2FA");
            this.ToastError("An error occurred while setting up two-factor authentication.");
            return RedirectToAction("Index", "Profile");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Setup(TwoFactorSetupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var request = new EnableTwoFactorRequest
            {
                VerificationCode = model.VerificationCode
            };

            var result = await _serviceManager.TwoFactorService.EnableTwoFactorAsync(request);

            if (!result.Successful)
            {
                ModelState.AddModelError("VerificationCode", result.Message ?? "Invalid verification code.");
                this.ToastError(result.Message ?? "Failed to enable two-factor authentication.");
                return View(model);
            }

            this.ToastSuccess("Two-factor authentication has been enabled successfully!");
            return RedirectToAction("BackupCodes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA");
            ModelState.AddModelError("", "An error occurred while enabling two-factor authentication.");
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Enable()
    {
        try
        {
            var statusResult = await _serviceManager.TwoFactorService.GetTwoFactorStatusAsync();

            if (statusResult.Data?.IsEnabled == true)
            {
                return Json(new
                {
                    success = true,
                    message = "Two-factor authentication is already enabled.",
                    setupRequired = false
                });
            }

            // Always require setup - don't enable without verification
            return Json(new
            {
                success = true,
                message = "Redirecting to setup...",
                setupRequired = true,
                setupUrl = Url.Action("Setup", "TwoFactor")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking 2FA status");
            this.ToastError("An error occurred while checking two-factor authentication status.");
            return Json(new
            {
                success = false,
                message = "An error occurred. Please try again."
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Disable()
    {
        try
        {
            var result = await _serviceManager.TwoFactorService.DisableTwoFactorAsync();

            return Json(new
            {
                success = result.Successful,
                message = result.Successful
                    ? "Two-factor authentication disabled successfully."
                    : result.Message ?? "Failed to disable two-factor authentication."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA");
            return Json(new
            {
                success = false,
                message = "An error occurred while disabling two-factor authentication."
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> BackupCodes()
    {
        try
        {
            var result = await _serviceManager.TwoFactorService.GenerateBackupCodesAsync();

            if (!result.Successful)
            {
                this.ToastError(result.Message ?? "Failed to generate backup codes.");
                return RedirectToAction("Index", "Profile");
            }

            var model = new BackupCodesViewModel
            {
                BackupCodes = result.Data?.BackupCodes ?? []
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes");
            this.ToastError("An error occurred while generating backup codes.");
            return RedirectToAction("Index", "Profile");
        }
    }

}
