using Asp.Versioning;

using ESSPortal.Application.Contracts.Interfaces.Common;

using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.TwoFactor;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESSPortal.Api.Controllers;



[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class TwoFactorController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly ILogger<TwoFactorController> _logger;

    public TwoFactorController(IServiceManager serviceManager, ILogger<TwoFactorController> logger)
    {
        _serviceManager = serviceManager;
        _logger = logger;
    }


    [HttpGet("setup-info")]
    public async Task<IActionResult> GetSetupInfo()
    {
        try
        {
            var result = await _serviceManager.TwoFactorService.GetSetupInfoAsync();
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA setup info");
            throw;
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var result = await _serviceManager.TwoFactorService.GetTwoFactorStatusAsync();
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA status");
            throw;
        }
    }
   
    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] EnableTwoFactorRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(AppResponse<bool>.Failure("Invalid request data."));
            }

            var result = await _serviceManager.TwoFactorService.EnableTwoFactorAsync(request);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA");
            throw;
        }
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable()
    {
        try
        {
            var result = await _serviceManager.TwoFactorService.DisableTwoFactorAsync();
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA");
            throw;
        }
    }

    [HttpPost("generate-backup-codes")]
    public async Task<IActionResult> GenerateBackupCodes()
    {
        try
        {
            var result = await _serviceManager.TwoFactorService.GenerateBackupCodesAsync();
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes");
            throw;
        }
    }

    [HttpPost("verify-totp")]
    public async Task<IActionResult> VerifyTotp([FromBody] VerifyTotpCodeRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(AppResponse<bool>.Failure("Invalid request data."));
            }

            var result = await _serviceManager.TwoFactorService.VerifyTotpCodeAsync(request);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying TOTP code");
            throw;
        }
    }




    private IActionResult HandleResponse<T>(AppResponse<T> response)
    {
        if (!response.Successful)
        {
            return Problem(
                detail: response.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: response.Message,
                instance: HttpContext.Request.Path
            );
        }
        return Ok(response);
    }
}
