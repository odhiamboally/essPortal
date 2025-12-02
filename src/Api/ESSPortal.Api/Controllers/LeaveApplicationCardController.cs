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
public class LeaveApplicationCardController : BaseController
{
    private readonly IServiceManager _serviceManager;

    public LeaveApplicationCardController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    // Read operations
    [HttpGet("")]
    public async Task<IActionResult> GetLeaveApplicationCards()
    {
        var response = await _serviceManager.LeaveApplicationCardService.GetLeaveApplicationCardsAsync();
        return HandleResponse(response);
    }

    [HttpGet("by-no/{applicationNo}")]
    public async Task<IActionResult> GetLeaveApplicationCardByNo(string applicationNo)
    {
        var response = await _serviceManager.LeaveApplicationCardService.GetLeaveApplicationCardByNoAsync(applicationNo);
        return HandleResponse(response);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchLeaveApplicationCards([FromBody] LeaveApplicationCardFilter filter)
    {
        var response = await _serviceManager.LeaveApplicationCardService.SearchLeaveApplicationCardsAsync(filter);
        return HandleResponse(response);
    }


    
}
