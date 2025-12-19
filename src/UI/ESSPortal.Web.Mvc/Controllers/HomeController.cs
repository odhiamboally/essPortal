using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Controllers;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Extensions;
using ESSPortal.Web.Mvc.Mappings;
using ESSPortal.Web.Mvc.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Controllers;

[Authorize]
public class HomeController : BaseController
{
    

    public HomeController(
        IServiceManager serviceManager, 
        IOptions<AppSettings> appSettings, 
        ILogger<AuthController> logger
        
        )
        : base(serviceManager, appSettings, logger)
    {
        
        
    }

    public async Task<IActionResult> Index(bool showPayslipModal = false, bool showP9Modal = false)
    {
        try
        {
            var employeeNo = _currentUser?.EmployeeNumber;

            var dashboardViewModel = CreateEmptyDashboardModel(employeeNo ?? string.Empty);

            if (string.IsNullOrWhiteSpace(employeeNo))
            {
                employeeNo = GetEmployeeNumber();
                //return RedirectToAction("SignIn", "Auth");
            }

            // Fix: Ensure employeeNo is not null before calling GetDashboard
            if (!string.IsNullOrWhiteSpace(employeeNo))
            {
                var cachedDashboardData = _serviceManager.CacheService.GetDashboard(employeeNo);
                if (cachedDashboardData != null)
                {
                    // If cached data exists, return it directly
                    ViewBag.ShowPayslipModal = showPayslipModal;
                    ViewBag.ShowP9Modal = showP9Modal;

                    dashboardViewModel = DashboardMappingExtensions.ToDashboardViewModel(cachedDashboardData);

                    return View(dashboardViewModel);
                }
            }

            var response = await _serviceManager.DashboardService.GetDashboardDataAsync(employeeNo ?? string.Empty);

            if (!response.Successful)
            {
                ViewBag.ErrorMessage = response.Message;
                ViewBag.ErrorType = "service_error";
                return View(CreateEmptyDashboardModel(employeeNo ?? string.Empty));
            }

            var dashboardData = response.Data ?? new(string.Empty, string.Empty, null, null, [], [], [], [], [], []);

            _serviceManager.CacheService.SetDashboard(employeeNo ?? string.Empty, dashboardData);

            dashboardViewModel = DashboardMappingExtensions.ToDashboardViewModel(dashboardData);

            if (dashboardViewModel == null)
            {
                ViewBag.ErrorMessage = "No dashboard data available.";
                ViewBag.ErrorType = "no_data";
                return View(CreateEmptyDashboardModel(employeeNo ?? string.Empty));
            }
            
            ViewBag.ShowPayslipModal = showPayslipModal;
            ViewBag.ShowP9Modal = showP9Modal;

            if (string.IsNullOrWhiteSpace(employeeNo))
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    // User is authenticated but we can't get their data
                    // Show an error, don't redirect (causes loop!)
                    ViewBag.ErrorMessage = "Unable to load your profile. Please try again or contact support.";
                    ViewBag.ErrorType = "profile_load_error";
                    return View(dashboardViewModel);
                }

                // Only redirect if truly not authenticated
                return RedirectToAction("SignIn", "Auth");
            }

            return View(dashboardViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard error for {EmployeeNo}", _currentUser?.EmployeeNumber);

            // Simple error handling
            ViewBag.ErrorMessage = "Unable to load dashboard. Please refresh the page.";
            ViewBag.ErrorType = "exception";

            return View(CreateEmptyDashboardModel(_currentUser?.EmployeeNumber ?? string.Empty));
        }
    }

    private DashboardViewModel CreateEmptyDashboardModel(string employeeNo)
    {
        return new DashboardViewModel
        {
            EmployeeNo = employeeNo,
            EmployeeName = $"{_currentUser?.FirstName} {_currentUser?.LastName}" ?? "Unknown User",
            AnnualLeaveSummary = new(),
            LeaveSummary = new(),
            LeaveApplicationCards = [],
            LeaveHistory = [],
            LeaveTypes = []
        };
    }


}
