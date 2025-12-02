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
public class LeaveRelieverController : BaseController
{
    private readonly IServiceManager _serviceManager;

    public LeaveRelieverController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    // Read operations
    [HttpGet("")]
    public async Task<IActionResult> GetLeaveRelievers()
    {
        var response = await _serviceManager.LeaveRelieversService.GetLeaveRelieversAsync();
        return HandleResponse(response);
    }

    [HttpGet("by-composite")]
    public async Task<IActionResult> GetLeaveRelieverByComposite([FromQuery] string leaveCode, [FromQuery] string staffNo)
    {
        var response = await _serviceManager.LeaveRelieversService.GetLeaveRelieverAsync(leaveCode, staffNo);
        return HandleResponse(response);
    }

    [HttpGet("by-application-no/{applicationNo}")]
    public async Task<IActionResult> GetLeaveRelieversByApplicationNo(string applicationNo)
    {
        var response = await _serviceManager.LeaveRelieversService.GetLeaveRelieversByApplicationNoAsync(applicationNo);
        return HandleResponse(response);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchLeaveRelievers([FromBody] LeaveRelieversFilter filter)
    {
        var response = await _serviceManager.LeaveRelieversService.SearchLeaveRelieversAsync(filter);
        return HandleResponse(response);
    }

    

    
}
