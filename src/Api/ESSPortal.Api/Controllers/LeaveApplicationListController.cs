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
public class LeaveApplicationListController : BaseController
{
    private readonly IServiceManager _serviceManager;

    public LeaveApplicationListController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    // Read operations
    [HttpGet("")]
    public async Task<IActionResult> GetLeaveApplicationLists()
    {
        var response = await _serviceManager.LeaveApplicationListService.GetLeaveApplicationListsAsync();
        return HandleResponse(response);
    }

    [HttpGet("by-no/{applicationNo}")]
    public async Task<IActionResult> GetLeaveApplicationListByNo(string applicationNo)
    {
        var response = await _serviceManager.LeaveApplicationListService.GetLeaveApplicationListByNoAsync(applicationNo);
        return HandleResponse(response);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchLeaveApplicationLists([FromBody] LeaveApplicationListFilter filter)
    {
        var response = await _serviceManager.LeaveApplicationListService.SearchLeaveApplicationListsAsync(filter);
        return HandleResponse(response);
    }

    

    
}
