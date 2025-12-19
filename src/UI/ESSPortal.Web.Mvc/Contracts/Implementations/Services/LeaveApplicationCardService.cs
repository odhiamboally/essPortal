using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Services;

internal sealed class LeaveApplicationCardService : ILeaveApplicationCardService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;

    public LeaveApplicationCardService(IApiService apiService, IOptions<ApiSettings> apiSettings)
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;
    }

    #region LeaveApplicationCard

    // Read operations
    public async Task<AppResponse<List<LeaveApplicationCard>>> GetLeaveApplicationCardsAsync()
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.GetLeaveApplicationCards;
        return await HandleGetRequest<List<LeaveApplicationCard>>(endpoint);
    }

    public async Task<AppResponse<LeaveApplicationCard?>> GetLeaveApplicationCardByNoAsync(string applicationNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.GetLeaveApplicationCardByNo;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "applicationNo", applicationNo } });
        return await HandleGetRequest<LeaveApplicationCard?>(endpoint);
    }

    public async Task<AppResponse<LeaveApplicationCard?>> GetLeaveApplicationCardByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.GetLeaveApplicationCardByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await HandleGetRequest<LeaveApplicationCard?>(endpoint);
    }

    public async Task<AppResponse<List<LeaveApplicationCard>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.SearchLeaveApplicationCards;
        return await HandlePostRequest<LeaveApplicationCardFilter, List<LeaveApplicationCard>>(endpoint, filter);
    }

    // Create operations
    public async Task<AppResponse<LeaveApplicationCard>> CreateLeaveApplicationCardAsync(LeaveApplicationCard request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.CreateLeaveApplicationCard;
        return await HandlePostRequest<LeaveApplicationCard, LeaveApplicationCard>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveApplicationCard>>> CreateMultipleLeaveApplicationCardsAsync(List<LeaveApplicationCard> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.CreateMultipleLeaveApplicationCards;
        return await HandlePostRequest<List<LeaveApplicationCard>, List<LeaveApplicationCard>>(endpoint, requests);
    }

    // Update operations
    public async Task<AppResponse<LeaveApplicationCard>> EditLeaveApplicationCardAsync(LeaveApplicationCard request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.EditLeaveApplicationCard;
        return await HandlePutRequest<LeaveApplicationCard, LeaveApplicationCard>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveApplicationCard>>> UpdateMultipleLeaveApplicationCardsAsync(List<LeaveApplicationCard> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.UpdateMultipleLeaveApplicationCards;
        return await HandlePutRequest<List<LeaveApplicationCard>, List<LeaveApplicationCard>>(endpoint, requests);
    }

    // Delete operations
    public async Task<AppResponse<bool>> DeleteLeaveApplicationCardAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.DeleteLeaveApplicationCard;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleDeleteRequest<bool>(endpoint);
    }

    // Utility operations
    public async Task<AppResponse<string?>> GetLeaveApplicationCardRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.GetLeaveApplicationCardRecIdFromKey;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleGetRequest<string?>(endpoint);
    }

    public async Task<AppResponse<bool>> IsLeaveApplicationCardUpdatedAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationCard.IsLeaveApplicationCardUpdated;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleGetRequest<bool>(endpoint);
    }

    // Helper methods
    private async Task<AppResponse<T>> HandleGetRequest<T>(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return AppResponse<T>.Failure("Endpoint not configured.");

        endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
        var apiResponse = await _apiService.GetAsync<T>(endpoint);

        return apiResponse.Successful
            ? AppResponse<T>.Success(apiResponse.Message!, apiResponse.Data!)
            : AppResponse<T>.Failure(apiResponse.Message!);
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

    private async Task<AppResponse<T>> HandleDeleteRequest<T>(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return AppResponse<T>.Failure("Endpoint not configured.");

        endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
        var apiResponse = await _apiService.DeleteAsync<T>(endpoint);

        return apiResponse.Successful
            ? AppResponse<T>.Success(apiResponse.Message!, apiResponse.Data!)
            : AppResponse<T>.Failure(apiResponse.Message!);
    }

    #endregion
}
