using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Dtos.Common;
using EssPortal.Web.Blazor.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Models.Navision;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Employee;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using ESSPortal.Web.Blazor.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.Services;

internal sealed class LeaveStatisticsFactboxService : ILeaveStatisticsFactboxService
{
    private readonly IServiceManager _serviceManager;
    private readonly ILogger<LeaveStatisticsFactboxService> _logger;
    public LeaveStatisticsFactboxService(IServiceManager serviceManager, ILogger<LeaveStatisticsFactboxService> logger)
    {
        _serviceManager = serviceManager;
        _logger = logger;
    }


    public async Task<ApiResponse<PagedResult<LeaveStatisticsFactboxResponse>>> GetLeaveStatisticsAsync()
    {
        try
        {
            var apiResponse = await _serviceManager.LeaveStatisticsFactboxService.GetLeaveStatisticsAsync();

            return apiResponse.Successful 
                ? ApiResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new ())
                : ApiResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Failure(apiResponse.Message!);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ApiResponse<PagedResult<LeaveStatisticsFactboxResponse>>> SearchLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter)
    {
        try
        {
            var apiResponse = await _serviceManager.LeaveStatisticsFactboxService.SearchLeaveStatisticsAsync(filter);

            return apiResponse.Successful
                ? ApiResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new())
                : ApiResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Failure(apiResponse.Message!);
        }
        catch (Exception)
        {

            throw;
        }
        
    }

    
}
