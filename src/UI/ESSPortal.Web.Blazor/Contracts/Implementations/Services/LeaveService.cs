using Azure;

using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using ESSPortal.Web.Blazor.Dtos.Leave;
using ESSPortal.Web.Blazor.Utilities.Api;

using Microsoft.Extensions.Options;

using System.ServiceModel.Channels;

using IServiceManager = ESSPortal.Application.Contracts.Interfaces.Common.IServiceManager;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.Services;

internal sealed class LeaveService : ILeaveService
{
    private readonly IServiceManager _serviceManager;
    private readonly IAppStateService _stateService;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(IServiceManager serviceManager, IAppStateService stateService, ILogger<LeaveService> logger)
    {
        _serviceManager = serviceManager;
        _stateService = stateService;
        _logger = logger;
    }

    public async Task<ApiResponse<LeaveApplicationResponse>> CreateLeaveApplicationAsync(CreateLeaveApplicationRequest request)
    {
        try
        {
            var response = await _serviceManager.LeaveService.CreateLeaveApplicationAsync(request);

            if (!response.Successful)
            {
                
                _logger.LogWarning("Failed to create leave application for {EmployeeNo}: {Message}", request.EmployeeNo, response.Message);
            }
            else
            {
                // Clear leave form cache so it refreshes with new data
                _stateService.ClearLeaveFormCache();
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave application for {EmployeeNo}", request.EmployeeNo);

            throw;
                
        }
    }

    public async Task<ApiResponse<LeaveApplicationResponse>> EditLeaveApplicationAsync(CreateLeaveApplicationRequest request)
    {
        try
        {
            var response = await _serviceManager.LeaveService.UpdateLeaveApplicationAsync(request);

            if (!response.Successful)
            {
                _logger.LogWarning("Failed to update leave for {EmployeeNo}: {Message}", request.EmployeeNo, response.Message);
            }
            else
            {
                _stateService.ClearLeaveFormCache();
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leave for {EmployeeNo}", request.EmployeeNo);
            throw;
                
        }

    }
            
    public async Task<ApiResponse<LeaveSummaryResponse>> GetLeaveSummaryAsync(string employeeNo)
    {
        try
        {
            var response = await _serviceManager.LeaveService.GetLeaveSummaryAsync(employeeNo);
            if (!response.Successful)
            {
                _logger.LogWarning("Failed to get leave summary for {EmployeeNo}: {Message}", employeeNo, response.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave summary for {EmployeeNo}", employeeNo);
            throw;
        }
    }

    public async Task<ApiResponse<AnnualLeaveSummaryResponse>> GetAnnualLeaveSummaryAsync(string employeeNo)
    {
        try
        {
            var response = await _serviceManager.LeaveService.GetAnnualLeaveSummaryAsync(employeeNo);
            if (!response.Successful)
            {
                _logger.LogWarning("Failed to get annual leave summary for {EmployeeNo}: {Message}", employeeNo, response.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting annual leave summary for {EmployeeNo}", employeeNo);
            throw;
        }
    }

    public async Task<ApiResponse<PagedResult<LeaveHistoryResponse>>> GetLeaveHistoryAsync(string employeeNo)
    {
        try
        {
            var response = await _serviceManager.LeaveService.GetLeaveHistoryAsync(employeeNo);
            if (!response.Successful)
            {
                _logger.LogWarning("Failed to get leave history for {EmployeeNo}: {Message}", employeeNo, response.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave history for {EmployeeNo}", employeeNo);
            throw;
        }
    }

    public async Task<ApiResponse<LeaveApplicationFormResponse>> GetLeaveApplicationFormDataAsync(string employeeNo)
    {
        try
        {
            _logger.LogInformation("Getting form data for {EmployeeNo}", employeeNo);

            // This uses cached dashboard data - super fast!
            var formData = await _stateService.GetLeaveApplicationFormDataAsync(employeeNo);
            if (formData == null)
            {
                _logger.LogWarning("No form data found in cache for {EmployeeNo}", employeeNo);
                return ApiResponse<LeaveApplicationFormResponse>.Failure("No form data found");
            }

            return ApiResponse<LeaveApplicationFormResponse>.Success("Form data loaded successfully", formData);
                
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting form data for {EmployeeNo}", employeeNo);
            throw;
                
        }
    }

   
}
