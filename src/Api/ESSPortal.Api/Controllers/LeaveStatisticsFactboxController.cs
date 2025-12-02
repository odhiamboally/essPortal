using Asp.Versioning;

using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Contracts.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESSPortal.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class LeaveStatisticsFactboxController : BaseController
{
    private readonly IServiceManager _serviceManager;

    public LeaveStatisticsFactboxController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    // Read operations (Statistics are typically read-only)
    [HttpGet("")]
    public async Task<IActionResult> GetLeaveStatistics()
    {
        var response = await _serviceManager.LeaveStatisticsFactboxService.GetLeaveStatisticsAsync();
        return HandleResponse(response);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchLeaveStatistics([FromBody] LeaveStatisticsFactboxFilter filter)
    {
        var response = await _serviceManager.LeaveStatisticsFactboxService.SearchLeaveStatisticsAsync(filter);
        return HandleResponse(response);
    }


    
}
