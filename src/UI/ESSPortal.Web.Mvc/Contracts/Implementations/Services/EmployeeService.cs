using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Dtos.Employee;
using ESSPortal.Web.Mvc.Utilities.Api;
using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Services;

internal sealed class EmployeeService : IEmployeeService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;

    public EmployeeService(IApiService apiService, IOptions<ApiSettings> apiSettings)
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;
    }

    public async Task<AppResponse<List<Employees>>> GetEmployeesAsync()
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.Employees;
        return await HandleGetRequest<List<Employees>>(endpoint);
    }

    public async Task<AppResponse<Employees>> GetEmployeeByNoAsync(string employeeNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeByNo;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "employeeNo", employeeNo } });
        return await HandleGetRequest<Employees>(endpoint);
    }

    public async Task<AppResponse<Employees>> GetEmployeeByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await HandleGetRequest<Employees>(endpoint);
    }

    public async Task<AppResponse<List<Employees>>> SearchEmployeesAsync(EmployeesFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.SearchEmployees;
        return await HandlePostRequest<EmployeesFilter, List<Employees>>(endpoint, filter);
    }

    public async Task<AppResponse<List<EmployeeCard>>> GetEmployeeCardsAsync()
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        return await HandleGetRequest<List<EmployeeCard>>(endpoint);
    }

    public async Task<AppResponse<EmployeeCard>> GetEmployeeCardByNoAsync(string employeeNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCardByNo;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "employeeNo", employeeNo } });
        return await HandleGetRequest<EmployeeCard>(endpoint);
    }

    public async Task<AppResponse<EmployeeCard>> GetEmployeeCardByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCardByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await HandleGetRequest<EmployeeCard>(endpoint);
    }

    public async Task<AppResponse<bool>> CreateEmployeeCardAsync(CreateEmployeeCardRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        return await HandlePostRequest<CreateEmployeeCardRequest, bool>(endpoint, request);
    }

    public async Task<AppResponse<bool>> UpdateEmployeeCardAsync(CreateEmployeeCardRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        return await HandlePutRequest<CreateEmployeeCardRequest, bool>(endpoint, request);
    }

    public async Task<AppResponse<bool>> DeleteEmployeeCardAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleDeleteRequest<bool>(endpoint);
    }

    public async Task<AppResponse<List<EmployeeCard>>> SearchEmployeeCardsAsync(EmployeeCardFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.SearchEmployeeCards;
        return await HandlePostRequest<EmployeeCardFilter, List<EmployeeCard>>(endpoint, filter);
    }

    public async Task<AppResponse<string>> GetRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleGetRequest<string>(endpoint);
    }

    public async Task<AppResponse<EmployeeCard>> GetUserEmailAsync(string? odataQuery = null)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        endpoint = string.IsNullOrWhiteSpace(odataQuery) ? endpoint : $"{endpoint}?{odataQuery}";
        return await HandleGetRequest<EmployeeCard>(endpoint);
    }

    public async Task<AppResponse<EmployeeCard>> CheckEmployeeNumberAsync(string? odataQuery = null)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        endpoint = string.IsNullOrWhiteSpace(odataQuery) ? endpoint : $"{endpoint}?{odataQuery}";
        return await HandleGetRequest<EmployeeCard>(endpoint);
    }



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
}
