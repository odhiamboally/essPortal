using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Controllers;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Dtos.Payroll;
using ESSPortal.Web.Mvc.Extensions;
using ESSPortal.Web.Mvc.ViewModels.Payroll;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Controllers;

[Authorize]
public class PayrollController : BaseController
{
    

    public PayrollController(
        IServiceManager serviceManager,
        IOptions<AppSettings> appSettings,
        ILogger<PayrollController> logger
        )
        : base(serviceManager, appSettings, logger)
    {
        
    }



    #region Payslip


    [HttpGet]
    public IActionResult PayslipModal(PayslipRequestViewModel? model = null)
    {
        if (model == null)
        {
            model = new PayslipRequestViewModel();
        }
        model.Month = DateTime.Now.Month;
        model.Year = DateTime.Now.Year;

        return PartialView("PayslipModal", model);
    }

    [HttpPost]
    public async Task<IActionResult> GeneratePayslip(PayslipRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new
            {
                success = false,
                message = "Validation failed: " + string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage))
            });
        }

        var employeeNumber = _currentUser?.EmployeeNumber;
        if (string.IsNullOrWhiteSpace(employeeNumber))
        {
            return Json(new
            {
                success = false,
                message = "Employee number is required." // Or a more user-friendly message
            });
        }

        PrintPaySlipRequest request = new PrintPaySlipRequest
        {
            EmployeeNo = employeeNumber,
            Year = model.Year,
            Month = model.Month
        };

        try
        {
            var (logoBase64, _, _) = await _serviceManager.FileService.ReadLogoAsync();

            _logger.LogInformation("Generating payslip for Employee: {EmployeeNo}, Month: {Month}, Year: {Year}",
                employeeNumber, model.Month, model.Year);

            var result = await _serviceManager.PayrollService.GeneratePayslipAsync(request);

            if (!result.Successful)
            {
                return Json(new
                {
                    success = false,
                    message = result.Message ?? "Failed to generate payslip"
                });
            }

            var fileBytes = result.Data!;

            // Return file information instead of FileResult
            return Json(new
            {
                success = true,
                fileName = $"Payslip_{employeeNumber}_{model.Year}_{model.Month:D2}.pdf",
                fileData = Convert.ToBase64String(fileBytes)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payslip");
            return Json(new
            {
                success = false,
                message = "An unexpected error occurred while generating the payslip."
            });
        }
    }

    [HttpGet("/Payroll/GetRecentPayslip")]
    public async Task<IActionResult> GetRecentPayslip()
    {
        try
        {
            var employeeNumber = _currentUser?.EmployeeNumber;
            if (string.IsNullOrWhiteSpace(employeeNumber))
            {
                _logger.LogWarning("Employee number is missing for recent payslip download");
                return Json(new
                {
                    success = false,
                    message = "Employee information is not available. Please sign in again."
                });
            }

            // Get the most recent payslip (previous month)
            var currentDate = DateTime.Now;
            var targetMonth = currentDate.Month - 1;
            var targetYear = currentDate.Year;

            // Handle year rollover
            if (targetMonth <= 0)
            {
                targetMonth = 12;
                targetYear--;
            }

            try
            {
                // Get logo for branding
                var (logoBase64, _, _) = await _serviceManager.FileService.ReadLogoAsync();

                PrintPaySlipRequest request = new PrintPaySlipRequest
                {
                    EmployeeNo = employeeNumber,
                    Year = targetYear,
                    Month = targetMonth,
                    LogoBase64 = logoBase64
                };

                _logger.LogInformation("Generating recent payslip for Employee: {EmployeeNo}, Month: {Month}, Year: {Year}",
                    employeeNumber, targetMonth, targetYear);

                var result = await _serviceManager.PayrollService.GeneratePayslipAsync(request);

                if (result == null)
                {
                    _logger.LogError("PayrollService returned null result for recent payslip - employee {EmployeeNo}", employeeNumber);
                    return Json(new
                    {
                        success = false,
                        message = "Unable to generate your recent payslip at this time. Please try the payslip generator instead."
                    });
                }

                if (!result.Successful)
                {
                    _logger.LogWarning("Recent payslip generation failed for employee {EmployeeNo}: {Message}",
                        employeeNumber, result.Message);

                    var userMessage = GetUserFriendlyErrorMessage(result.Message, "recent payslip");
                    return Json(new
                    {
                        success = false,
                        message = result.Message ?? $"{userMessage} Please try selecting a specific month and year using the payslip generator."
                    });
                }

                var fileBytes = result.Data!;
                _logger.LogInformation("Successfully generated recent payslip for employee {EmployeeNo}. Size: {Size} bytes",
                    employeeNumber, fileBytes.Length);

                var monthName = new DateTime(targetYear, targetMonth, 1).ToString("MMMM");
                var fileName = $"Payslip_{employeeNumber}_{targetYear}_{targetMonth:D2}_{monthName}.pdf";

                // Always return JSON with base64 data
                return Json(new
                {
                    success = true,
                    fileName,
                    fileData = Convert.ToBase64String(fileBytes)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recent payslip for employee {EmployeeNo}", employeeNumber);
                return Json(new
                {
                    success = false,
                    message = "Unable to generate recent payslip. Please try the payslip generator for a specific month and year."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetRecentPayslip for employee {EmployeeNo}",
                _currentUser?.EmployeeNumber ?? "Unknown");
            return Json(new
            {
                success = false,
                message = "An unexpected error occurred. Please try again."
            });
        }
    }

    [HttpGet("/Payroll/GetCurrentPayslip")]
    public async Task<IActionResult> GetCurrentPayslip()
    {
        try
        {
            var employeeNumber = _currentUser?.EmployeeNumber;
            if (string.IsNullOrWhiteSpace(employeeNumber))
            {
                _logger.LogWarning("Employee number is missing for current payslip download");
                return Json(new
                {
                    success = false,
                    message = "Employee information is not available. Please sign in again."
                });
            }

            // Get current month's payslip
            var currentDate = DateTime.Now;
            var targetMonth = currentDate.Month;
            var targetYear = currentDate.Year;

            try
            {
                // Get logo for branding
                var (logoBase64, _, _) = await _serviceManager.FileService.ReadLogoAsync();

                PrintPaySlipRequest request = new PrintPaySlipRequest
                {
                    EmployeeNo = employeeNumber,
                    Year = targetYear,
                    Month = targetMonth,
                    LogoBase64 = logoBase64
                };

                _logger.LogInformation("Generating current payslip for Employee: {EmployeeNo}, Month: {Month}, Year: {Year}",
                    employeeNumber, targetMonth, targetYear);

                var result = await _serviceManager.PayrollService.GeneratePayslipAsync(request);

                if (result == null)
                {
                    _logger.LogError("PayrollService returned null result for current payslip - employee {EmployeeNo}", employeeNumber);
                    return Json(new
                    {
                        success = false,
                        message = "Current month's payslip may not be available yet. Please try the payslip generator."
                    });
                }

                if (!result.Successful)
                {
                    _logger.LogWarning("Current payslip generation failed for employee {EmployeeNo}: {Message}",
                        employeeNumber, result.Message);

                    var userMessage = GetUserFriendlyErrorMessage(result.Message, "current payslip");
                    return Json(new
                    {
                        success = false,
                        message = $"{userMessage} Current month's payslip may not be available yet."
                    });
                }

                var fileBytes = result.Data!;
                _logger.LogInformation("Successfully generated current payslip for employee {EmployeeNo}. Size: {Size} bytes",
                    employeeNumber, fileBytes.Length);

                var monthName = new DateTime(targetYear, targetMonth, 1).ToString("MMMM");
                var fileName = $"Payslip_{employeeNumber}_{targetYear}_{targetMonth:D2}_{monthName}.pdf";

                // Always return JSON with base64 data
                return Json(new
                {
                    success = true,
                    fileName = fileName,
                    fileData = Convert.ToBase64String(fileBytes)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating current payslip for employee {EmployeeNo}", employeeNumber);
                return Json(new
                {
                    success = false,
                    message = "Unable to generate current payslip. Please try the payslip generator for a specific month and year."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetCurrentPayslip for employee {EmployeeNo}",
                _currentUser?.EmployeeNumber ?? "Unknown");
            return Json(new
            {
                success = false,
                message = "An unexpected error occurred. Please try again."
            });
        }
    }


    #endregion

    #region P9

    [HttpGet]
    public IActionResult P9Modal(P9RequestViewModel? model = null)
    {
        if (model == null)
        {
            model = new P9RequestViewModel();
        }

        model.Year = DateTime.Now.Year;

        return PartialView("P9Modal", model);
    }

    [HttpPost]
    public async Task<IActionResult> GenerateP9(P9RequestViewModel model)
    {
        var employeeNumber = _currentUser?.EmployeeNumber;
        model.EmployeeNumber = employeeNumber!;

        if (!ModelState.IsValid)
        {
            return Json(new
            {
                success = false,
                message = "Please fill in all required fields."
            });
        }

        
        if (string.IsNullOrWhiteSpace(employeeNumber))
        {
            return Json(new
            {
                success = false,
                message = "Employee number is required."
            });
        }

        try
        {
            PrintP9Request request = new PrintP9Request
            {
                EmployeeNo = employeeNumber,
                Year = model.Year
            };

            var (logoBase64, _, _) = await _serviceManager.FileService.ReadLogoAsync();

            _logger.LogInformation("Generating P9 for Employee: {EmployeeNo}, Year: {Year}", employeeNumber, model.Year);

            var result = await _serviceManager.PayrollService.GenerateP9Async(request);

            if (!result.Successful)
            {
                return Json(new
                {
                    success = false,
                    message = result.Message ?? "Failed to generate P9 certificate. Please try again."
                });
            }

            var fileBytes = result.Data!;
            var fileName = $"P9_{employeeNumber}_{model.Year}.pdf";

            return Json(new
            {
                success = true,
                fileName = fileName,
                fileData = Convert.ToBase64String(fileBytes)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating P9 for employee {EmployeeNo}", employeeNumber);
            return Json(new
            {
                success = false,
                message = "An unexpected error occurred while generating the P9 certificate."
            });
        }
    }

    [HttpGet("/Payroll/GetRecentP9")]
    public async Task<IActionResult> GetRecentP9()
    {
        try
        {
            var employeeNumber = _currentUser?.EmployeeNumber;
            if (string.IsNullOrWhiteSpace(employeeNumber))
            {
                _logger.LogWarning("Employee number is missing for recent P9 download");
                return Json(new
                {
                    success = false,
                    message = "Employee information is not available. Please sign in again."
                });
            }

            var currentDate = DateTime.Now;
            var targetYear = currentDate.Year - 1; // Default to previous year

            try
            {
                var (logoBase64, _, _) = await _serviceManager.FileService.ReadLogoAsync();

                PrintP9Request request = new PrintP9Request
                {
                    EmployeeNo = employeeNumber,
                    Year = targetYear,
                    LogoBase64 = logoBase64
                };

                _logger.LogInformation("Generating recent P9 for Employee: {EmployeeNo}, Year: {Year}",
                    employeeNumber, targetYear);

                var result = await _serviceManager.PayrollService.GenerateP9Async(request);

                if (result == null)
                {
                    _logger.LogError("PayrollService returned null result for recent P9 - employee {EmployeeNo}", employeeNumber);
                    return Json(new
                    {
                        success = false,
                        message = "Unable to generate your recent P9 certificate at this time. Please try the P9 generator instead."
                    });
                }

                if (!result.Successful)
                {
                    _logger.LogWarning("Recent P9 generation failed for employee {EmployeeNo}: {Message}",
                        employeeNumber, result.Message);

                    var userMessage = GetUserFriendlyErrorMessage(result.Message, "recent P9 certificate");
                    return Json(new
                    {
                        success = false,
                        message = $"{userMessage} Please try selecting a specific year using the P9 generator."
                    });
                }

                var fileBytes = result.Data!;
                _logger.LogInformation("Successfully generated recent P9 for employee {EmployeeNo}. Size: {Size} bytes",
                    employeeNumber, fileBytes.Length);

                var fileName = $"P9_{employeeNumber}_{targetYear}.pdf";

                return Json(new
                {
                    success = true,
                    fileName = fileName,
                    fileData = Convert.ToBase64String(fileBytes)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recent P9 for employee {EmployeeNo}", employeeNumber);
                return Json(new
                {
                    success = false,
                    message = "Unable to generate recent P9 certificate. Please try the P9 generator for a specific year."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetRecentP9 for employee {EmployeeNo}",
                _currentUser?.EmployeeNumber ?? "Unknown");
            return Json(new
            {
                success = false,
                message = "An unexpected error occurred. Please try again."
            });
        }
    }

    [HttpGet("/Payroll/GetCurrentP9")]
    public async Task<IActionResult> GetCurrentP9()
    {
        try
        {
            var employeeNumber = _currentUser?.EmployeeNumber;
            if (string.IsNullOrWhiteSpace(employeeNumber))
            {
                _logger.LogWarning("Employee number is missing for current P9 download");
                return Json(new
                {
                    success = false,
                    message = "Employee information is not available. Please sign in again."
                });
            }

            // Get current year's P9
            var currentYear = DateTime.Now.Year;

            try
            {
                // Get logo for branding
                var (logoBase64, _, _) = await _serviceManager.FileService.ReadLogoAsync();

                PrintP9Request request = new PrintP9Request
                {
                    EmployeeNo = employeeNumber,
                    Year = currentYear,
                    LogoBase64 = logoBase64
                };

                _logger.LogInformation("Generating current P9 for Employee: {EmployeeNo}, Year: {Year}",
                    employeeNumber, currentYear);

                var result = await _serviceManager.PayrollService.GenerateP9Async(request);

                if (result == null)
                {
                    _logger.LogError("PayrollService returned null result for current P9 - employee {EmployeeNo}", employeeNumber);
                    return Json(new
                    {
                        success = false,
                        message = "Current year's P9 certificate may not be available yet. Please try the P9 generator."
                    });
                }

                if (!result.Successful)
                {
                    _logger.LogWarning("Current P9 generation failed for employee {EmployeeNo}: {Message}",
                        employeeNumber, result.Message);

                    var userMessage = GetUserFriendlyErrorMessage(result.Message, "current P9 certificate");
                    return Json(new
                    {
                        success = false,
                        message = $"{userMessage} Current year's P9 may not be available yet."
                    });
                }

                var fileBytes = result.Data!;
                _logger.LogInformation("Successfully generated current P9 for employee {EmployeeNo}. Size: {Size} bytes",
                    employeeNumber, fileBytes.Length);

                var fileName = $"P9_{employeeNumber}_{currentYear}.pdf";

                return Json(new
                {
                    success = true,
                    fileName = fileName,
                    fileData = Convert.ToBase64String(fileBytes)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating current P9 for employee {EmployeeNo}", employeeNumber);
                return Json(new
                {
                    success = false,
                    message = "Unable to generate current P9 certificate. Please try the P9 generator for a specific year."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetCurrentP9 for employee {EmployeeNo}",
                _currentUser?.EmployeeNumber ?? "Unknown");
            return Json(new
            {
                success = false,
                message = "An unexpected error occurred. Please try again."
            });
        }
    }



    #endregion


    [HttpGet]
    public IActionResult DownloadTempFile(string fileName)
    {
        try
        {
            var fileDataKey = $"TempFile_{fileName}";
            var fileNameKey = $"TempFileName_{fileName}";

            if (!TempData.ContainsKey(fileDataKey) || !TempData.ContainsKey(fileNameKey))
            {
                _logger.LogWarning("Temp file not found: {FileName}", fileName);
                return NotFound("File not found or has expired.");
            }

            var base64Data = TempData[fileDataKey]?.ToString();
            var originalFileName = TempData[fileNameKey]?.ToString();

            if (string.IsNullOrWhiteSpace(base64Data) || string.IsNullOrWhiteSpace(originalFileName))
            {
                _logger.LogWarning("Invalid temp file data: {FileName}", fileName);
                return NotFound("Invalid file data.");
            }

            var fileBytes = Convert.FromBase64String(base64Data);

            _logger.LogInformation("Serving temp file: {OriginalFileName}, Size: {Size} bytes",
                originalFileName, fileBytes.Length);

            return File(fileBytes, "application/pdf", originalFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving temp file: {FileName}", fileName);
            return StatusCode(500, "Error retrieving file.");
        }
    }

    private string GetUserFriendlyErrorMessage(string? serverMessage, string documentType)
    {
        if (string.IsNullOrWhiteSpace(serverMessage))
        {
            return $"Unable to generate {documentType} at this time. Please try again later.";
        }

        var lowerMessage = serverMessage.ToLowerInvariant();

        // Common error patterns and user-friendly translations
        if (lowerMessage.Contains("not found") || lowerMessage.Contains("no data"))
        {
            return $"No {documentType} data available for the selected period. Please contact HR if you believe this is an error.";
        }

        if (lowerMessage.Contains("connection") || lowerMessage.Contains("timeout") || lowerMessage.Contains("network"))
        {
            return "Service temporarily unavailable. Please try again in a few minutes.";
        }

        if (lowerMessage.Contains("unauthorized") || lowerMessage.Contains("permission"))
        {
            return "You don't have permission to access this document. Please contact your administrator.";
        }

        if (lowerMessage.Contains("business central") || lowerMessage.Contains("bc"))
        {
            return serverMessage;
        }

        if (lowerMessage.Contains("invalid") || lowerMessage.Contains("format"))
        {
            return "Invalid request parameters. Please refresh the page and try again.";
        }

        // For any other server error, return a generic message
        return $"Unable to generate {documentType} at this time. Please try again later or contact support if the problem persists.";
    }

}


    

