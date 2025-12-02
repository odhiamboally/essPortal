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
public class EmployeeController : BaseController
{
    private readonly IServiceManager _serviceManager;
    private readonly IConfiguration? _configuration;
   
    public EmployeeController(
        IServiceManager serviceManager, 
        IConfiguration configuration
        )
    {
        _serviceManager = serviceManager;
        _configuration = configuration;
    }

    // Employees
    [HttpGet("")]
    public async Task<IActionResult> GetAllEmployees()
    {
        var response = await _serviceManager.EmployeeService.GetEmployeesAsync();
        return HandleResponse(response);
    }

    [HttpGet("{employeeNo}")]
    public async Task<IActionResult> GetEmployeeByNumber(string employeeNo)
    {
        var response = await _serviceManager.EmployeeService.GetEmployeeByNoAsync(employeeNo);
        return HandleResponse(response);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchEmployees([FromBody] EmployeesFilter filter)
    {
        var response = await _serviceManager.EmployeeService.SearchEmployeesAsync(filter);
        return HandleResponse(response);
    }

    // Employee Cards
    [HttpGet("cards")]
    public async Task<IActionResult> GetAllEmployeeCards()
    {
        var response = await _serviceManager.EmployeeService.GetEmployeeCardsAsync();
        return HandleResponse(response);
    }

    [HttpGet("cards/{employeeNo}")]
    public async Task<IActionResult> GetEmployeeCardByNumber(string employeeNo)
    {
        var response = await _serviceManager.EmployeeService.GetEmployeeCardByNoAsync(employeeNo);
        return HandleResponse(response);
    }

    [HttpPost("cards/search")]
    public async Task<IActionResult> SearchEmployeeCards([FromBody] EmployeeCardFilter filter)
    {
        var response = await _serviceManager.EmployeeService.SearchEmployeeCardsAsync(filter);
        return HandleResponse(response);
    }


    //private IActionResult HandleResponse<T>(ApiResponse<T> response)
    //{
    //    if (!response.Successful)
    //    {
    //        return Problem(
    //            detail: response.Message,
    //            statusCode: StatusCodes.Status500InternalServerError,
    //            title: "Operation Failed",
    //            instance: HttpContext.Request.Path
    //        );
    //    }
    //    return Ok(response);
    //}
}
