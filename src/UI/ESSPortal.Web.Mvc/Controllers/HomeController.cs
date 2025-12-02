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
                return RedirectToAction("SignIn", "Auth");
            }

            var cachedDashboardData = _serviceManager.CacheService.GetDashboard(employeeNo);
            if (cachedDashboardData != null)
            {
                // If cached data exists, return it directly
                ViewBag.ShowPayslipModal = showPayslipModal;
                ViewBag.ShowP9Modal = showP9Modal;

                dashboardViewModel = DashboardMappingExtensions.ToDashboardViewModel(cachedDashboardData);

                return View(dashboardViewModel);

            }

            var response = await _serviceManager.DashboardService.GetDashboardDataAsync(employeeNo);

            if (!response.Successful)
            {
                ViewBag.ErrorMessage = response.Message;
                ViewBag.ErrorType = "service_error";
                return View(CreateEmptyDashboardModel(employeeNo));
            }

            var dashboardData = response.Data ?? new(string.Empty, string.Empty, null, null, [], [], [], [], [], []);

            _serviceManager.CacheService.SetDashboard(employeeNo, dashboardData);

            dashboardViewModel = DashboardMappingExtensions.ToDashboardViewModel(dashboardData);

            if (dashboardViewModel == null)
            {
                ViewBag.ErrorMessage = "No dashboard data available.";
                ViewBag.ErrorType = "no_data";
                return View(CreateEmptyDashboardModel(employeeNo));
            }
            
            ViewBag.ShowPayslipModal = showPayslipModal;
            ViewBag.ShowP9Modal = showP9Modal;

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
