using Asp.Versioning;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;
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
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLeaveApplication([FromBody] CreateLeaveApplicationRequest request)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<LeaveApplicationResponse>.Failure("Invalid leave application request"));
        }

        var response = await _serviceManager.LeaveService.CreateLeaveApplicationAsync(request);
        
        return HandleResponse(response);
    }

    // Create Leave Application
    [HttpPut("update")]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLeaveApplication([FromBody] CreateLeaveApplicationRequest request)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<LeaveApplicationResponse>.Failure("Invalid leave application request"));
        }

        var response = await _serviceManager.LeaveService.UpdateLeaveApplicationAsync(request);

        return HandleResponse(response);
    }

    [HttpPost("leave-history")]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LeaveHistory([FromBody] string employeeNo)
    {
        if (string.IsNullOrWhiteSpace(employeeNo))
        {
            return BadRequest(ApiResponse<LeaveHistoryResponse>.Failure("Invalid leave application request"));
        }

        var response = await _serviceManager.LeaveService.GetLeaveHistoryAsync(employeeNo);

        return HandleResponse(response);
    }

    [HttpPost("annual-leave-summary")]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnnualLeaveSummary([FromBody] string employeeNo)
    {
        if (string.IsNullOrWhiteSpace(employeeNo))
        {
            return BadRequest(ApiResponse<LeaveHistoryResponse>.Failure("Invalid leave application request"));
        }

        var response = await _serviceManager.LeaveService.GetAnnualLeaveSummaryAsync(employeeNo);

        return HandleResponse(response);
    }

    [HttpPost("leave-summary")]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<LeaveHistoryResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LeaveSummary([FromBody] string employeeNo)
    {
        if (string.IsNullOrWhiteSpace(employeeNo))
        {
            return BadRequest(ApiResponse<LeaveHistoryResponse>.Failure("Invalid leave application request"));
        }

        var response = await _serviceManager.LeaveService.GetLeaveSummaryAsync(employeeNo);

        return HandleResponse(response);
    }

    


}
