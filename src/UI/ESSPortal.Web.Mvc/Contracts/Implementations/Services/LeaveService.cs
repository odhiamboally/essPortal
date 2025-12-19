using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Dtos.Leave;
using ESSPortal.Web.Mvc.Utilities.Api;
using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Services;

internal sealed class LeaveService : ILeaveService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;

    public LeaveService(IApiService apiService, IOptions<ApiSettings> apiSettings)
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;
    }

    public async Task<AppResponse<LeaveApplicationResponse>> CreateLeaveApplicationAsync(CreateLeaveApplicationRequest request)
    {

        var endpoint = _apiSettings.ApiEndpoints.Leave.CreateLeaveApplication;
        return await HandlePostRequest<CreateLeaveApplicationRequest, LeaveApplicationResponse>(endpoint, request);
    }

    public Task<AppResponse<LeaveApplicationResponse>> EditLeaveApplicationAsync(CreateLeaveApplicationRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.Leave.EditLeaveApplication;
        return HandlePutRequest<CreateLeaveApplicationRequest, LeaveApplicationResponse>(endpoint, request);

    }

    public async Task<AppResponse<LeaveSummaryResponse>> GetLeaveSummaryAsync(string employeeNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.Leave.GetLeaveSummary;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "employeeNo", employeeNo } });
        return await HandlePostRequest<string, LeaveSummaryResponse>(endpoint, employeeNo);
    }

    public async Task<AppResponse<PagedResult<LeaveHistoryResponse>>> GetLeaveHistoryAsync(string employeeNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.Leave.GetLeaveHistory;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "employeeNo", employeeNo } });
        return await HandlePostRequest<string, PagedResult<LeaveHistoryResponse>>(endpoint, employeeNo);
    }



    private async Task<AppResponse<TResponse>> HandlePostRequest<TRequest, TResponse>(string endpoint, TRequest request)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return AppResponse<TResponse>.Failure("Endpoint not configured.");

        endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
        var apiResponse = await _apiService.PostAsync<TRequest, TResponse>(endpoint, request);

        return apiResponse.Successful
            ? AppResponse<TResponse>.Success(apiResponse.Message!, apiResponse.Data!)
            : AppResponse<TResponse>.Failure(apiResponse.Message!);
    }

    private async Task<AppResponse<TResponse>> HandlePutRequest<TRequest, TResponse>(string endpoint, TRequest request)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return AppResponse<TResponse>.Failure("Endpoint not configured.");

        endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
        var apiResponse = await _apiService.PutAsync<TRequest, TResponse>(endpoint, request);

        return apiResponse.Successful
            ? AppResponse<TResponse>.Success(apiResponse.Message!, apiResponse.Data!)
            : AppResponse<TResponse>.Failure(apiResponse.Message!);
    }

}
