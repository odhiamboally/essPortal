using Asp.Versioning;
using ESSPortal.Application.Contracts.Interfaces.Common;

using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESSPortal.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class LeaveController : BaseController
{
    private readonly IServiceManager _serviceManager;

    public LeaveController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;

    }


    // Create Leave Application
    [HttpPost("create")]
    [ProducesResponseType(typeof(AppResponse<LeaveApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<LeaveApplicationResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<LeaveApplicationResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<LeaveApplicationResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponse<LeaveApplicationResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLeaveApplication([FromBody] CreateLeaveApplicationRequest request)
    {
        if (request == null)
        {
            return BadRequest(AppResponse<LeaveApplicationResponse>.Failure("Invalid leave application request"));
        }

        var response = await _serviceManager.LeaveService.CreateLeaveApplicationAsync(request);
        
        return HandleResponse(response);
    }

    // Create Leave Application
    [HttpPut("update")]
    [ProducesResponseType(typeof(AppResponse<LeaveApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<LeaveApplicationResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<LeaveApplicationResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<LeaveApplicationResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponse<LeaveApplicationResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLeaveApplication([FromBody] CreateLeaveApplicationRequest request)
    {
        if (request == null)
        {
            return BadRequest(AppResponse<LeaveApplicationResponse>.Failure("Invalid leave application request"));
        }

        var response = await _serviceManager.LeaveService.UpdateLeaveApplicationAsync(request);

        return HandleResponse(response);
    }

    [HttpPost("leave-history")]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LeaveHistory([FromBody] string employeeNo)
    {
        if (string.IsNullOrWhiteSpace(employeeNo))
        {
            return BadRequest(AppResponse<LeaveHistoryResponse>.Failure("Invalid leave application request"));
        }

        var response = await _serviceManager.LeaveService.GetLeaveHistoryAsync(employeeNo);

        return HandleResponse(response);
    }

    [HttpPost("annual-leave-summary")]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnnualLeaveSummary([FromBody] string employeeNo)
    {
        if (string.IsNullOrWhiteSpace(employeeNo))
        {
            return BadRequest(AppResponse<LeaveHistoryResponse>.Failure("Invalid leave application request"));
        }

        var response = await _serviceManager.LeaveService.GetAnnualLeaveSummaryAsync(employeeNo);

        return HandleResponse(response);
    }

    [HttpPost("leave-summary")]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponse<LeaveHistoryResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LeaveSummary([FromBody] string employeeNo)
    {
        if (string.IsNullOrWhiteSpace(employeeNo))
        {
            return BadRequest(AppResponse<LeaveHistoryResponse>.Failure("Invalid leave application request"));
        }

        var response = await _serviceManager.LeaveService.GetLeaveSummaryAsync(employeeNo);

        return HandleResponse(response);
    }

    


}
