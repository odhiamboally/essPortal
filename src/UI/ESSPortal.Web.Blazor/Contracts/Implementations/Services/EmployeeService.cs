using Azure.Core;

using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Dtos.Common;
using EssPortal.Web.Blazor.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Models.Navision;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Employee;
using ESSPortal.Domain.NavEntities;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using ESSPortal.Web.Blazor.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.Services;

internal sealed class EmployeeService : IEmployeeService
{
    private readonly IServiceManager _serviceManager;
    public EmployeeService(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<ApiResponse<PagedResult<EmployeeResponse>>> GetEmployeesAsync()
    {
        var apiResponse = await _serviceManager.EmployeeService.GetEmployeesAsync();

        return apiResponse.Successful
            ? ApiResponse<PagedResult<EmployeeResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new PagedResult<EmployeeResponse>())
            : ApiResponse<PagedResult<EmployeeResponse>>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<EmployeeResponse>> GetEmployeeByNoAsync(string employeeNo)
    {
        var apiResponse = await _serviceManager.EmployeeService.GetEmployeeByNoAsync(employeeNo);

        return apiResponse.Successful
            ? ApiResponse<EmployeeResponse>.Success(apiResponse.Message!, apiResponse.Data ?? new EmployeeResponse())
            : ApiResponse<EmployeeResponse>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<PagedResult<EmployeeResponse>>> SearchEmployeesAsync(EmployeesFilter filter)
    {
        var apiResponse = await _serviceManager.EmployeeService.SearchEmployeesAsync(filter);

        return apiResponse.Successful
            ? ApiResponse<PagedResult<EmployeeResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new PagedResult<EmployeeResponse>())
            : ApiResponse<PagedResult<EmployeeResponse>>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<bool>> CreateEmployeeCardAsync(CreateEmployeeCardRequest request)
    {
        var apiResponse = await _serviceManager.EmployeeService.CreateEmployeeCardsAsync(request);

        return apiResponse.Successful
            ? ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data)
            : ApiResponse<bool>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<PagedResult<EmployeeCardResponse>>> GetEmployeeCardsAsync()
    {
        var apiResponse = await _serviceManager.EmployeeService.GetEmployeeCardsAsync();

        return apiResponse.Successful
            ? ApiResponse<PagedResult<EmployeeCardResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new PagedResult<EmployeeCardResponse>())
            : ApiResponse<PagedResult<EmployeeCardResponse>>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<EmployeeCardResponse>> GetEmployeeCardByNoAsync(string employeeNo)
    {
        var apiResponse = await _serviceManager.EmployeeService.GetEmployeeCardByNoAsync(employeeNo);

        return apiResponse.Successful
            ? ApiResponse<EmployeeCardResponse>.Success(apiResponse.Message!, apiResponse.Data ?? new ())
            : ApiResponse<EmployeeCardResponse>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<PagedResult<EmployeeCardResponse>>> SearchEmployeeCardsAsync(EmployeeCardFilter filter)
    {
        var apiResponse = await _serviceManager.EmployeeService.SearchEmployeeCardsAsync(filter);

        return apiResponse.Successful
            ? ApiResponse<PagedResult<EmployeeCardResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new ())
            : ApiResponse<PagedResult<EmployeeCardResponse>>.Failure(apiResponse.Message!);
    }

    
}
