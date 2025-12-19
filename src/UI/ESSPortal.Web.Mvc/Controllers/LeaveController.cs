using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Controllers;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Dtos.Dashboard;
using ESSPortal.Web.Mvc.Dtos.Leave;
using ESSPortal.Web.Mvc.Extensions;
using ESSPortal.Web.Mvc.Utilities.Session;
using ESSPortal.Web.Mvc.Validations.RequestValidators.Leave;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace ESSPortal.Web.Mvc.Controllers;

[Authorize]
public class LeaveController : BaseController
{
    
    private const string SessionKey_UserInfo = "UserInfo";

    public LeaveController(
        IServiceManager serviceManager,
        IOptions<AppSettings> appSettings,
        ILogger<LeaveController> logger) : base(serviceManager, appSettings, logger)
        
        
    {
        
    }


    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ApplyForLeave()
    {
        try
        {
            var employeeNo = _currentUser?.EmployeeNumber;
            if (string.IsNullOrWhiteSpace(employeeNo))
            {
                return RedirectToAction("SignIn", "Auth");
            }

            // Get cached dashboard data (should already be available)
            var cachedDashboardData = _serviceManager.CacheService.GetDashboard(employeeNo);
            if (cachedDashboardData == null)
            {
                _logger.LogInformation("No cached dashboard data found for employee {EmployeeNo}, fetching fresh data", employeeNo);

                var dashboardResponse = await _serviceManager.DashboardService.GetDashboardDataAsync(employeeNo);

                if (!dashboardResponse.Successful || dashboardResponse.Data == null)
                {
                    _logger.LogError("Failed to load dashboard data for employee {EmployeeNo}: {Message}", employeeNo, dashboardResponse.Message);

                  this.ToastError("Unable to load required data for leave application. Please try refreshing your dashboard first.");

                    return RedirectToAction("Index", "Home");
                }

                cachedDashboardData = dashboardResponse.Data;
            }

            var userInfo = GetUserInfoFromSession();

            var formResponse = new LeaveApplicationFormResponse
            {
                Employee = new LeaveApplicationEmployeeResponse
                {
                    EmployeeNo = employeeNo,
                    EmployeeName = $"{_currentUser?.FirstName} {_currentUser?.LastName}".Trim(),
                    EmailAddress = _currentUser?.Email ?? string.Empty,
                    MobileNo = _currentUser?.PhoneNumber ?? string.Empty, 
                    ResponsibilityCenter = userInfo?.ResponsibilityCenter ??cachedDashboardData.LeaveApplicationCards.FirstOrDefault()?.ResponsibilityCenter ?? string.Empty,
                   
                    BranchCode = "NAIROBI" 
                },

                AnnualLeaveSummary = cachedDashboardData?.AnnualLeaveSummary ?? new(),
                LeaveSummary = cachedDashboardData?.LeaveSummary ?? new(),
                LeaveTypes = cachedDashboardData?.LeaveTypes ?? [],
                AvailableRelievers = cachedDashboardData?.LeaveRelievers ?? []
            };

            return View(formResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading leave application form for {EmployeeNo}", _currentUser?.EmployeeNumber);
            this.ToastError("Unable to load leave application form. Please try again.");
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ApplyForLeave([FromForm] CreateLeaveApplicationRequest request)
    {
        bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                         || Request.Headers["Accept"].ToString().Contains("application/json");

        try
        {
            var userInfo = GetUserInfoFromSession();
            if (userInfo == null) 
            {
                userInfo = CacheServiceExtensions.GetUserInfo(_serviceManager.CacheService, _currentUser?.EmployeeNumber ?? string.Empty);

                if (userInfo == null)
                {
                    _logger.LogWarning("User info not found in session for employee {EmployeeNo}", _currentUser?.EmployeeNumber);
                }
            }

            request = request with
            {
                EmployeeNo = _currentUser?.EmployeeNumber ?? string.Empty,
                EmailAddress = _currentUser?.Email ?? string.Empty,
                EmploymentType = userInfo?.EmploymentType ?? string.Empty,
                ResponsibilityCenter = userInfo?.ResponsibilityCenter ?? string.Empty,
            };

            if (request.Attachments != null && request.Attachments.Length > 0)
            {
                var fileValidationErrors = ValidateUploadedFiles(request.Attachments);
                if (fileValidationErrors.Count > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "File validation failed",
                        errors = fileValidationErrors
                    });
                }
            }

            var cachedDashboardData = _serviceManager.CacheService.GetDashboard(request.EmployeeNo);
            if (cachedDashboardData == null)
            {
                _logger.LogInformation("No cached dashboard data found for employee {EmployeeNo}, fetching fresh data", request.EmployeeNo);

                var dashboardResponse = await _serviceManager.DashboardService.GetDashboardDataAsync(request.EmployeeNo);
                if (!dashboardResponse.Successful || dashboardResponse.Data == null)
                {
                    if (isAjaxRequest)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Unable to load required data for leave application."
                        });
                    }

                    this.ToastWarning("Unable to load required data for leave application.", "Data Load Error");
                    var errorFormResponse = await BuildLeaveApplicationFormResponse(request);
                    return View("ApplyForLeave", errorFormResponse);

                }

                cachedDashboardData = dashboardResponse.Data;
            }

            if (request.SelectedRelieverEmployeeNos.Count > 1)
            {
                if (isAjaxRequest)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please select a reliever for your leave application."
                    });
                }

                this.ToastWarning("Please select only one reliever for your leave application.", "Multiple Relievers Selected");

                var errorFormResponse = await BuildLeaveApplicationFormResponse(request, cachedDashboardData);
                return View("ApplyForLeave", errorFormResponse);
                
            }

            if (request.SelectedRelieverEmployeeNos.Count == 0)
            {
                this.ToastWarning("Please select a reliever for your leave application.", "No Reliever Selected");
                var errorFormResponse = await BuildLeaveApplicationFormResponse(request, cachedDashboardData);
                return View("ApplyForLeave", errorFormResponse);
            }

            var selectedReliever = cachedDashboardData?.LeaveRelievers?.FirstOrDefault(r =>
                r.EmployeeNo == request.SelectedRelieverEmployeeNos.First());

            if (selectedReliever != null)
            {
                request = request with
                {
                    DutiesTakenOverBy = selectedReliever.EmployeeNo,
                    RelievingName = selectedReliever.EmployeeName
                };
            }

            bool isEditing = false;
            var validator = new CreateLeaveApplicationRequestValidator(_serviceManager, _currentUser?.Gender ?? string.Empty, isEditing);

            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                this.ToastWarning($"{string.Join(", ", errors)}", "Validation Failed");

                var errorFormResponse = await BuildLeaveApplicationFormResponse(request, cachedDashboardData);

                if (isAjaxRequest)
                {
                    return Json(new
                    {
                        success = false,
                        message = errors.FirstOrDefault(),
                        errors = errors,
                        redirectUrl = Url.Action("Index", "Home")
                    });
                }

                return View("ApplyForLeave", errorFormResponse);

            }

            var result = await _serviceManager.LeaveService.CreateLeaveApplicationAsync(request);

            if (result.Successful)
            {
                _serviceManager.CacheService.InvalidateAllUserCaches(_currentUser?.EmployeeNumber!);

                if (isAjaxRequest)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Leave application submitted successfully",
                        applicationNo = result.Data?.ApplicationNo,
                        redirectUrl = Url.Action("Index", "Home")
                    });
                }

                this.ToastActivitySuccess("Leave Application", "Leave application submitted successfully");

                return RedirectToAction("Index", "Home");

            }
            else
            {
                if (isAjaxRequest)
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message ?? "Failed to submit application",
                        applicationNo = result.Data?.ApplicationNo,
                        redirectUrl = Url.Action("Index", "Home")
                    });
                }

                this.ToastError(result.Message ?? "Failed to submit application", "Submission Error");
                var errorFormResponse = await BuildLeaveApplicationFormResponse(request, cachedDashboardData);
                return View("ApplyForLeave", errorFormResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting leave application for {EmployeeNo}", _currentUser?.EmployeeNumber);

            if (isAjaxRequest)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while submitting your application"
                });
            }

            this.ToastError("An error occurred while submitting your application", "Error");
            var errorFormResponse = await BuildLeaveApplicationFormResponse(request);
            return View("ApplyForLeave", errorFormResponse);

        }
    }

    [HttpGet]
    public async Task<IActionResult> EditLeaveModal(string applicationNo)
    {
        // PSEUDOCODE / PLAN:
        // 1. Validate input: if applicationNo is null/empty -> return BadRequest.
        // 2. Retrieve leave history by calling the existing LeaveHistory() action.
        // 3. Ensure the returned IActionResult is a ViewResult and contains a List<LeaveHistoryResponse>.
        // 4. Find the leaveDetails matching the provided applicationNo. If not found -> return NotFound.
        // 5. Build a LeaveApplicationFormResponse for the modal by calling BuildLeaveApplicationFormResponse with a prefilled CreateLeaveApplicationRequest.
        // 6. Compute the resumption date:
        //    - Start with the day after the leave end date: endDate + 1 day.
        //    - While the candidate resumption date falls on a weekend (Saturday or Sunday), advance by 1 day.
        //    - (Note: holiday checking is intentionally omitted to avoid coupling to an unknown holiday source;
        //       if holiday data becomes available in the dashboard or via a service, this loop can also check against that list.)
        //    - Format the resumption date as "yyyy-MM-dd" to match existing ViewBag date strings.
        // 7. Populate ViewBag with existing leave data (type, dates, days, half-day flag, reliever, allowance payable).
        // 8. Return the partial view "_EditLeaveModal" with the built formResponse.
        //
        // Implementation details:
        // - Use safe null checks for _currentUser.
        // - Use the existing logging and error handling pattern in the controller where appropriate.
        // - Keep the method asynchronous because BuildLeaveApplicationFormResponse is async.

        if (string.IsNullOrWhiteSpace(applicationNo))
        {
            return BadRequest("Application number is required");
        }

        // Get the leave details by application number
        var leaveHistory = await LeaveHistory();
        if (leaveHistory == null)
        {
            return BadRequest("Unable to retrieve leave data");
        }

        // Ensure leaveHistory is a ViewResult and contains the expected model type
        if (leaveHistory is not ViewResult viewResult || viewResult.Model is not List<LeaveHistoryResponse> leaveHistoryList)
        {
            return BadRequest("Unable to retrieve leave data");
        }

        var leaveDetails = leaveHistoryList.FirstOrDefault(l => l.ApplicationNo == applicationNo);

        if (leaveDetails == null)
        {
            return NotFound("Leave application not found");
        }

        var formResponse = await BuildLeaveApplicationFormResponse(new CreateLeaveApplicationRequest
        {
            ApplicationNo = applicationNo,
            EmployeeNo = _currentUser?.EmployeeNumber ?? string.Empty,
            EmployeeName = $"{_currentUser?.FirstName} {_currentUser?.LastName}".Trim(),
            EmailAddress = _currentUser?.Email ?? string.Empty,
            MobileNo = _currentUser?.PhoneNumber ?? string.Empty,
            DutiesTakenOverBy = leaveDetails.DutiesTakenOverBy,
            HalfDay = leaveDetails.DaysApplied == 0.5m || leaveDetails.Duration == 0.5,
            LeaveAllowancePayable = leaveDetails.Duration > 10,
        });

        // Pass existing data via ViewBag so it can be bound in the view
        ViewBag.ApplicationNo = applicationNo;
        ViewBag.ExistingLeaveType = leaveDetails.LeaveType;
        ViewBag.ExistingFromDate = leaveDetails.StartDate.ToString("yyyy-MM-dd");
        ViewBag.ExistingToDate = leaveDetails.EndDate.ToString("yyyy-MM-dd");
        ViewBag.ExistingDaysApplied = leaveDetails.DaysApplied;
        ViewBag.DutiesTakenOverBy = leaveDetails.DutiesTakenOverBy;
        ViewBag.ExistingHalfDay = leaveDetails.DaysApplied == 0.5m || leaveDetails.Duration == 0.5;

        // Calculate resumption date as the day after end date, skipping weekends (Saturday/Sunday).
        // If holiday awareness is added later, also skip holidays in this loop.
        try
        {
            var resumptionDate = leaveDetails.EndDate.Date.AddDays(1);

            while (resumptionDate.DayOfWeek == DayOfWeek.Saturday || resumptionDate.DayOfWeek == DayOfWeek.Sunday)
            {
                resumptionDate = resumptionDate.AddDays(1);
            }

            ViewBag.ExistingResumptionDate = resumptionDate.ToString("yyyy-MM-dd");
        }
        catch
        {
            // Fallback: at minimum provide the day after end date formatted
            ViewBag.ExistingResumptionDate = leaveDetails.EndDate.Date.AddDays(1).ToString("yyyy-MM-dd");
        }

        ViewBag.ExistingLeaveAllowancePayable = leaveDetails.Duration > 10;

        return PartialView("_EditLeaveModal", formResponse);
    }

    [HttpPost]
    public async Task<IActionResult> EditLeaveApplication([FromForm] CreateLeaveApplicationRequest request)
    {
        try
        {
            var userInfo = GetUserInfoFromSession();

            request = request with
            {
                EmployeeNo = _currentUser?.EmployeeNumber ?? string.Empty,
                EmailAddress = _currentUser?.Email ?? string.Empty,
                EmploymentType = userInfo?.EmploymentType ?? string.Empty,
                ResponsibilityCenter = userInfo?.ResponsibilityCenter ?? string.Empty,
            };

            if (request.Attachments != null && request.Attachments.Length > 0)
            {
                var fileValidationErrors = ValidateUploadedFiles(request.Attachments);
                if (fileValidationErrors.Count > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "File validation failed",
                        errors = fileValidationErrors
                    });
                }
            }

            var cachedDashboardData = _serviceManager.CacheService.GetDashboard(request.EmployeeNo);
            if (cachedDashboardData == null)
            {
                _logger.LogInformation("No cached dashboard data found for employee {EmployeeNo}, fetching fresh data", request.EmployeeNo);

                var dashboardResponse = await _serviceManager.DashboardService.GetDashboardDataAsync(request.EmployeeNo);
                if (!dashboardResponse.Successful || dashboardResponse.Data == null)
                {
                    this.ToastWarning("Unable to load required data for leave application.", "Data Load Error");

                    var errorFormResponse = await BuildLeaveApplicationFormResponse(request);
                    return View("ApplyForLeave", errorFormResponse);

                }

                cachedDashboardData = dashboardResponse.Data;
            }

            if (request.SelectedRelieverEmployeeNos.Count > 1)
            {
                this.ToastWarning("Please select only one reliever for your leave application.", "Multiple Relievers Selected");

                var errorFormResponse = await BuildLeaveApplicationFormResponse(request, cachedDashboardData);
                return View("ApplyForLeave", errorFormResponse);

            }

            if (request.SelectedRelieverEmployeeNos.Count == 0)
            {
                this.ToastWarning("Please select a reliever for your leave application.", "No Reliever Selected");

                var errorFormResponse = await BuildLeaveApplicationFormResponse(request, cachedDashboardData);

                return View("ApplyForLeave", errorFormResponse);
            }

            var selectedReliever = cachedDashboardData?.LeaveRelievers?.FirstOrDefault(r =>
                r.EmployeeNo == request.SelectedRelieverEmployeeNos.First());

            if (selectedReliever != null)
            {
                request = request with
                {
                    DutiesTakenOverBy = selectedReliever.EmployeeNo,
                    RelievingName = selectedReliever.EmployeeName
                };
            }

            bool isEditing = true;
            var validator = new CreateLeaveApplicationRequestValidator(_serviceManager, _currentUser?.Gender ?? string.Empty, isEditing);

            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                this.ToastWarning($"{string.Join(", ", errors)}", "Validation Failed");

                var errorFormResponse = await BuildLeaveApplicationFormResponse(request, cachedDashboardData);

                return View("ApplyForLeave", errorFormResponse);

            }

            var result = await _serviceManager.LeaveService.EditLeaveApplicationAsync(request);

            if (result.Successful)
            {
                _serviceManager.CacheService.InvalidateAllUserCaches(_currentUser?.EmployeeNumber!);
                return Json(new
                {
                    success = true,
                    message = "Leave application updated successfully",
                    redirectUrl = Url.Action("LeaveHistory", "Leave")
                });
            }
            else
            {
                return Json(new { success = false, message = result.Message ?? "Failed to update application" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leave application {ApplicationNo}", request.ApplicationNo);
            return Json(new { success = false, message = "An error occurred while updating your application" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> LeaveDetailModal(string applicationNo)
    {
        if (string.IsNullOrWhiteSpace(applicationNo))
        {
            return BadRequest("Application number is required");
        }

        // Get the leave details by application number
        var leaveHistory = await LeaveHistory();
        if (leaveHistory == null)
        {
            return BadRequest("Unable to retrieve leave history");
        }

        // Ensure leaveHistory is a ViewResult and contains the expected model type
        if (leaveHistory is not ViewResult viewResult || viewResult.Model is not List<LeaveHistoryResponse> leaveHistoryList)
        {
            return BadRequest("Unable to retrieve leave history");
        }
        var leaveDetails = leaveHistoryList.FirstOrDefault(l => l.ApplicationNo == applicationNo);

        if (leaveDetails == null)
        {
            return NotFound("Leave application not found");
        }

        return PartialView("_LeaveDetailModal", leaveDetails);
    }

    [HttpGet]
    public async Task<IActionResult> LeaveHistory()
    {
        try
        {
            var employeeNo = _currentUser?.EmployeeNumber;
            if (string.IsNullOrWhiteSpace(employeeNo))
            {
                return RedirectToAction("SignIn", "Auth");
            }

            // Get cached leave history data
            var cachedHistoryData = _serviceManager.CacheService.GetLeaveHistory(employeeNo);
            if (cachedHistoryData == null)
            {
                _logger.LogInformation("No cached leave history found for employee {EmployeeNo}, fetching fresh data", employeeNo);

                var historyResponse = await _serviceManager.LeaveService.GetLeaveHistoryAsync(employeeNo);
                if (!historyResponse.Successful || historyResponse.Data == null)
                {
                    this.ToastError("Unable to load leave history. Please try again later.");
                    return RedirectToAction("Index", "Home");
                }

                cachedHistoryData = historyResponse.Data.Items.OrderByDescending(lh => lh.ApplicationNo).ToList();
            }

            return View(cachedHistoryData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading leave history for {EmployeeNo}", _currentUser?.EmployeeNumber);
            this.ToastError("Unable to load leave history. Please try again.");

            return RedirectToAction("Index", "Home");
        }
    }

    private List<string> ValidateUploadedFiles(IFormFile[] files)
    {
        var errors = new List<string>();
        const long maxFileSize = 10 * 1024 * 1024; // 10MB

        string[] allowedExtensions =
        [
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                                  ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar"
        ];

        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                errors.Add($"File '{file.FileName}' is empty");
                continue;
            }

            if (file.Length > maxFileSize)
            {
                errors.Add($"File '{file.FileName}' exceeds maximum size of 10MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                errors.Add($"File '{file.FileName}' has an unsupported file type");
            }
        }

        if (files.Length > 10)
        {
            errors.Add("Maximum of 10 files can be uploaded");
        }

        return errors;
    }

    private async Task<LeaveApplicationFormResponse> BuildLeaveApplicationFormResponse(
        CreateLeaveApplicationRequest request,
        object? cachedDashboardData = null)
    {
        try
        {
            var employeeNo = request.EmployeeNo ?? _currentUser?.EmployeeNumber ?? string.Empty;

            if (cachedDashboardData == null)
            {
                cachedDashboardData = _serviceManager.CacheService.GetDashboard(employeeNo);

                if (cachedDashboardData == null)
                {
                    var dashboardResponse = await _serviceManager.DashboardService.GetDashboardDataAsync(employeeNo);
                    cachedDashboardData = dashboardResponse?.Data;
                }
            }

            var userInfo = GetUserInfoFromSession();

            // Cast to the expected type 
            var dashboardData = cachedDashboardData as DashboardResponse; 

            return new LeaveApplicationFormResponse
            {
                Employee = new LeaveApplicationEmployeeResponse
                {
                    EmployeeNo = employeeNo,
                    EmployeeName = !string.IsNullOrEmpty(request.EmployeeName)
                        ? request.EmployeeName
                        : $"{_currentUser?.FirstName} {_currentUser?.LastName}".Trim(),

                    EmailAddress = !string.IsNullOrEmpty(request.EmailAddress)
                        ? request.EmailAddress
                        : _currentUser?.Email ?? string.Empty,

                    MobileNo = !string.IsNullOrEmpty(request.MobileNo)
                        ? request.MobileNo
                        : _currentUser?.PhoneNumber ?? string.Empty,

                    ResponsibilityCenter = !string.IsNullOrEmpty(request.ResponsibilityCenter)
                        ? request.ResponsibilityCenter
                        : userInfo?.ResponsibilityCenter ?? dashboardData?.LeaveApplicationCards?.FirstOrDefault()?.ResponsibilityCenter ?? string.Empty,

                    BranchCode = "NAIROBI"
                },
                AnnualLeaveSummary = dashboardData?.AnnualLeaveSummary ?? new(),
                LeaveSummary = dashboardData?.LeaveSummary ?? new(),
                LeaveTypes = dashboardData?.LeaveTypes ?? [],
                AvailableRelievers = dashboardData?.LeaveRelievers ?? []
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building leave application form response for {EmployeeNo}", request.EmployeeNo);

            // Return a minimal form response in case of error
            return new LeaveApplicationFormResponse
            {
                Employee = new LeaveApplicationEmployeeResponse
                {
                    EmployeeNo = request.EmployeeNo,
                    EmployeeName = request.EmployeeName,
                    EmailAddress = request.EmailAddress,
                    MobileNo = request.MobileNo,
                    ResponsibilityCenter = request.ResponsibilityCenter,
                    BranchCode = "NAIROBI"
                }
            };
        }
    }



}
