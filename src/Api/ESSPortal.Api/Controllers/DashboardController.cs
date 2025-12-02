using Asp.Versioning;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Dashboard;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESSPortal.Api.Controllers;
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly ILogger<DashboardController> _logger;
    public DashboardController(IServiceManager serviceManager, ILogger<DashboardController> logger)
    {
        _serviceManager = serviceManager;
        _logger = logger;
    }

    [HttpGet("{employeeNo}")]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status500InternalServerError)]

    public async Task<IActionResult> GetDashboardData(string employeeNo)
    {
        try
        {
            var response = await _serviceManager.DashboardService.GetDashboardDataAsync(employeeNo);
            if (!response.Successful)
            {
                return Problem(
                    detail: response.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Failed to fetch dashboard records",
                    instance: HttpContext.Request.Path
                );
            }

            if (response.Data == null)
            {
                return NotFound(ApiResponse<DashboardResponse>.Failure("No dashboard data found for the specified employee number"));
            }

            return Ok(ApiResponse<DashboardResponse>.Success("Dashboard data retrieved successfully", response.Data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data for employee {EmployeeNo}", employeeNo);
            return StatusCode(500, ApiResponse<DashboardResponse>.Failure("An error occurred while retrieving dashboard data"));
        }
    }
}
