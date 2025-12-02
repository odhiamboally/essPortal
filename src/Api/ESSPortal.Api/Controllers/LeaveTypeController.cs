using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Contracts.Interfaces.Common;
using Microsoft.AspNetCore.Mvc;

namespace ESSPortal.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class LeaveTypeController : BaseController
{
    private readonly IServiceManager _serviceManager;

    public LeaveTypeController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var response = await _serviceManager.LeaveTypeService.GetLeaveTypesAsync();
        return HandleResponse(response);
    }

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetLeaveTypeByCode(string code)
    {
        var response = await _serviceManager.LeaveTypeService.GetLeaveTypeByCodeAsync(code);
        return HandleResponse(response);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchLeaveTypes([FromBody] LeaveTypeFilter filter)
    {
        var response = await _serviceManager.LeaveTypeService.SearchLeaveTypesAsync(filter);
        return HandleResponse(response);
    }

    
}
