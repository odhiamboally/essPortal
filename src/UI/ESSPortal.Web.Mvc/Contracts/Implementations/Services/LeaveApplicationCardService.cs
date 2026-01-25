using EssPortal.Shared.Configurations;
using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;
using ESSPortal.Shared.Utilities.Api;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

using Microsoft.Extensions.Options;

namespace ESSPortal.Shared.Contracts.Implementations.Services;

internal sealed class LeaveApplicationCardService(
    IServiceManager serviceManager,
    IApiService apiService, 
    IOptions<ApiSettings> apiSettings
    
) : ILeaveApplicationCardService
{
    private readonly ApiSettings _apiSettings = apiSettings.Value;

    #region LeaveApplicationCard

    // Read operations
    public async Task<AppResponse<List<LeaveApplicationCardResponse>>> GetLeaveApplicationCardsAsync()
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.GetLeaveApplicationCards;
        return await apiService.HandleGetRequest<List<LeaveApplicationCardResponse>>(endpoint);
    }

    public async Task<AppResponse<LeaveApplicationCardResponse?>> GetLeaveApplicationCardByNoAsync(string applicationNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.GetLeaveApplicationCardByNo;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "applicationNo", applicationNo } });
        return await apiService.HandleGetRequest<LeaveApplicationCardResponse?>(endpoint);
    }

    public async Task<AppResponse<LeaveApplicationCardResponse?>> GetLeaveApplicationCardByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.GetLeaveApplicationCardByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await apiService.HandleGetRequest<LeaveApplicationCardResponse?>(endpoint);
    }

    public async Task<AppResponse<List<LeaveApplicationCardResponse>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.SearchLeaveApplicationCards;
        return await apiService.HandlePostRequest<LeaveApplicationCardFilter, List<LeaveApplicationCardResponse>>(endpoint, filter);
    }

    // Create operations
    public async Task<AppResponse<LeaveApplicationCardResponse>> CreateLeaveApplicationCardAsync(CreateLeaveApplicationCardRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.CreateLeaveApplicationCard;
        return await apiService.HandlePostRequest<CreateLeaveApplicationCardRequest, LeaveApplicationCardResponse>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveApplicationCardResponse>>> CreateMultipleLeaveApplicationCardsAsync(List<CreateLeaveApplicationCardRequest> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.CreateMultipleLeaveApplicationCards;
        return await apiService.HandlePostRequest<List<CreateLeaveApplicationCardRequest>, List<LeaveApplicationCardResponse>>(endpoint, requests);
    }

    // Update operations
    public async Task<AppResponse<LeaveApplicationCardResponse>> EditLeaveApplicationCardAsync(CreateLeaveApplicationCardRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.EditLeaveApplicationCard;
        return await apiService.HandlePutRequest<CreateLeaveApplicationCardRequest, LeaveApplicationCardResponse>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveApplicationCardResponse>>> UpdateMultipleLeaveApplicationCardsAsync(List<CreateLeaveApplicationCardRequest> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.UpdateMultipleLeaveApplicationCards;
        return await apiService.HandlePutRequest<List<CreateLeaveApplicationCardRequest>, List<LeaveApplicationCardResponse>>(endpoint, requests);
    }

    // Delete operations
    public async Task<AppResponse<bool>> DeleteLeaveApplicationCardAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.DeleteLeaveApplicationCard;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleDeleteRequest<bool>(endpoint);
    }

    // Utility operations
    public async Task<AppResponse<string?>> GetLeaveApplicationCardRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.GetLeaveApplicationCardRecIdFromKey;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<string?>(endpoint);
    }

    public async Task<AppResponse<bool>> IsLeaveApplicationCardUpdatedAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.IsLeaveApplicationCardUpdated;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<bool>(endpoint);
    }

    

    #endregion
}
