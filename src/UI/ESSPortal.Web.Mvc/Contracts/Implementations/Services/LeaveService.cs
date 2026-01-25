using EssPortal.Shared.Configurations;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;

using ESSPortal.Shared.Dtos.Leave;
using ESSPortal.Shared.Utilities.Api;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

using Microsoft.Extensions.Options;

namespace ESSPortal.Shared.Contracts.Implementations.Services;

internal sealed class LeaveService(
    IServiceManager serviceManager,
    IApiService apiService, 
    IOptions<ApiSettings> apiSettings

) : ILeaveService
{
    private readonly ApiSettings _apiSettings = apiSettings.Value;

    public async Task<AppResponse<LeaveApplicationResponse>> CreateLeaveApplicationAsync(CreateLeaveApplicationRequest request)
    {

        var endpoint = _apiSettings.ApiEndpoints.Leave.CreateLeaveApplication;
        return await apiService.HandlePostRequest<CreateLeaveApplicationRequest, LeaveApplicationResponse>(endpoint, request);
    }

    public Task<AppResponse<LeaveApplicationResponse>> EditLeaveApplicationAsync(CreateLeaveApplicationRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.Leave.EditLeaveApplication;
        return apiService.HandlePutRequest<CreateLeaveApplicationRequest, LeaveApplicationResponse>(endpoint, request);

    }

    public async Task<AppResponse<LeaveSummaryResponse>> GetLeaveSummaryAsync(string employeeNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.Leave.GetLeaveSummary;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "employeeNo", employeeNo } });
        return await apiService.HandlePostRequest<string, LeaveSummaryResponse>(endpoint, employeeNo);
    }

    public async Task<AppResponse<PagedResult<LeaveHistoryResponse>>> GetLeaveHistoryAsync(string employeeNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.Leave.GetLeaveHistory;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "employeeNo", employeeNo } });
        return await apiService.HandlePostRequest<string, PagedResult<LeaveHistoryResponse>>(endpoint, employeeNo);
    }



    

}
