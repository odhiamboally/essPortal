using EssPortal.Shared.Configurations;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Employee;
using ESSPortal.Shared.Utilities.Api;
using Microsoft.Extensions.Options;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using EssPortal.Shared.Dtos.Employee;

namespace ESSPortal.Shared.Contracts.Implementations.Services;

internal sealed class EmployeeService(
    IServiceManager serviceManager,
    IApiService apiService,
    IOptions<ApiSettings> apiSettings

) : IEmployeeService
{
    private readonly ApiSettings _apiSettings = apiSettings.Value;

    public async Task<AppResponse<List<EmployeeResponse>>> GetEmployeesAsync()
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.Employees;
        return await apiService.HandleGetRequest<List<EmployeeResponse>>(endpoint);
    }

    public async Task<AppResponse<EmployeeResponse>> GetEmployeeByNoAsync(string employeeNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeByNo;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "employeeNo", employeeNo } });
        return await apiService.HandleGetRequest<EmployeeResponse>(endpoint);
    }

    public async Task<AppResponse<EmployeeResponse>> GetEmployeeByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await apiService.HandleGetRequest<EmployeeResponse>(endpoint);
    }

    public async Task<AppResponse<List<EmployeeResponse>>> SearchEmployeesAsync(EmployeesFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.SearchEmployees;
        return await apiService.HandlePostRequest<EmployeesFilter, List<EmployeeResponse>>(endpoint, filter);
    }

    public async Task<AppResponse<List<EmployeeCardResponse>>> GetEmployeeCardsAsync()
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        return await apiService.HandleGetRequest<List<EmployeeCardResponse>>(endpoint);
    }

    public async Task<AppResponse<EmployeeCardResponse>> GetEmployeeCardByNoAsync(string employeeNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCardByNo;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "employeeNo", employeeNo } });
        return await apiService.HandleGetRequest<EmployeeCardResponse>(endpoint);
    }

    public async Task<AppResponse<EmployeeCardResponse>> GetEmployeeCardByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCardByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await apiService.HandleGetRequest<EmployeeCardResponse>(endpoint);
    }

    public async Task<AppResponse<bool>> CreateEmployeeCardAsync(CreateEmployeeCardRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        return await apiService.HandlePostRequest<CreateEmployeeCardRequest, bool>(endpoint, request);
    }

    public async Task<AppResponse<bool>> UpdateEmployeeCardAsync(CreateEmployeeCardRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        return await apiService.HandlePutRequest<CreateEmployeeCardRequest, bool>(endpoint, request);
    }

    public async Task<AppResponse<bool>> DeleteEmployeeCardAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<bool>(endpoint);
    }

    public async Task<AppResponse<List<EmployeeCardResponse>>> SearchEmployeeCardsAsync(EmployeeCardFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.SearchEmployeeCards;
        return await apiService.HandlePostRequest<EmployeeCardFilter, List<EmployeeCardResponse>>(endpoint, filter);
    }

    public async Task<AppResponse<string>> GetRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<string>(endpoint);
    }

    public async Task<AppResponse<EmployeeCardResponse>> GetUserEmailAsync(string? odataQuery = null)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        endpoint = string.IsNullOrWhiteSpace(odataQuery) ? endpoint : $"{endpoint}?{odataQuery}";
        return await apiService.HandleGetRequest<EmployeeCardResponse>(endpoint);
    }

    public async Task<AppResponse<EmployeeCardResponse>> CheckEmployeeNumberAsync(string? odataQuery = null)
    {
        var endpoint = _apiSettings.ApiEndpoints.Employee.EmployeeCards;
        endpoint = string.IsNullOrWhiteSpace(odataQuery) ? endpoint : $"{endpoint}?{odataQuery}";
        return await apiService.HandleGetRequest<EmployeeCardResponse>(endpoint);
    }



    
}
