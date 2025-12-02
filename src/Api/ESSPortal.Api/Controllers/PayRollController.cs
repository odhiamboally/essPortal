using Asp.Versioning;


using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Payroll;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESSPortal.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class PayRollController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly ILogger<PayRollController> _logger;


    public PayRollController(IServiceManager serviceManager, ILogger<PayRollController> logger)
    {
        _serviceManager = serviceManager;
        _logger = logger;

    }

    [HttpPost("GeneratePayslip")]
    public async Task<IActionResult> GeneratePayslipAsync([FromBody] PrintPaySlipRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<byte[]>.Failure("Invalid request data."));
            }

            var result = await _serviceManager.PayrollService.GeneratePayslipAsync(request);

            if (!result.Successful)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payslip for employee {EmployeeNo}", request.EmployeeNo);
            return StatusCode(500, ApiResponse<byte[]>.Failure("An error occurred while generating payslip."));
        }
    }

    [HttpPost("GenerateP9")]
    public async Task<IActionResult> GenerateP9Async([FromBody] PrintP9Request request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<byte[]>.Failure("Invalid request data."));
            }

            var result = await _serviceManager.PayrollService.GenerateP9Async(request);

            if (!result.Successful)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating P9 for employee {EmployeeNo}", request.EmployeeNo);
            return StatusCode(500, ApiResponse<byte[]>.Failure("An error occurred while generating P9."));
        }
    }

    

}
