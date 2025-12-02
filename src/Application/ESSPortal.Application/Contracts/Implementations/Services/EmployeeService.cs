using AutoMapper;

using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Configuration;

using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Extensions;
using ESSPortal.Application.Utilities;
using ESSPortal.Domain.NavEntities;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class EmployeeService : IEmployeeService
{
    private readonly INavisionService _navisionService;
    private readonly ILogger<EmployeeService> _logger;
    private readonly BCSettings _bcSettings;
    private readonly IMapper _mapper;

    public EmployeeService(
        ILogger<EmployeeService> logger,
        IOptions<BCSettings> bcSettings,
        INavisionService navisionService,
        IMapper mapper)
    {
        _logger = logger;
        _bcSettings = bcSettings.Value;
        _navisionService = navisionService;
        _mapper = mapper;
    }

    // Employees
    public async Task<ApiResponse<PagedResult<Employees>>> GetEmployeesAsync()
    {
        if (!_bcSettings.EntitySets.TryGetValue("Employees", out var entitySet))
            return ApiResponse<PagedResult<Employees>>.Failure("Entity set not configured");

        var response = await _navisionService.GetMultipleAsync<Employees>(entitySet);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    public async Task<ApiResponse<Employees>> GetEmployeeByNoAsync(string employeeNo)
    {
        if (!_bcSettings.EntitySets.TryGetValue("Employees", out var entitySet))
            return ApiResponse<Employees>.Failure("Entity set not configured");

        var requestUri = $"{entitySet}?$filter=No eq '{employeeNo}'";
        var response = await _navisionService.GetSingleAsync<Employees>(requestUri);

        if (!response.Successful)
            return ApiResponse<Employees>.Failure(response.Message ?? "Failed to fetch employee");

        if (response.Data == null)
            return ApiResponse<Employees>.Failure("Employee not found");

        return ApiResponse<Employees>.Success("Success", response.Data);
    }

    public async Task<ApiResponse<PagedResult<Employees>>> SearchEmployeesAsync(EmployeesFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("Employees", out var entitySet))
            return ApiResponse<PagedResult<Employees>>.Failure("Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<Employees>(requestUri);
        if (!response.Successful)
            return ApiResponse<PagedResult<Employees>>.Failure(response.Message ?? "Failed to fetch employee records");

        // Extract the list from the tuple
        var (_, _) = response.Data;

        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    public async Task<ApiResponse<PagedResult<EmployeeCard>>> GetEmployeeCardsAsync()
    {
        if (!_bcSettings.EntitySets.TryGetValue("EmployeeCards", out var entitySet))
            return ApiResponse<PagedResult<EmployeeCard>>.Failure("Entity set not configured");

        var response = await _navisionService.GetMultipleAsync<EmployeeCard>(entitySet);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    public async Task<ApiResponse<EmployeeCard>> GetEmployeeCardByNoAsync(string employeeNo)
    {
        if (!_bcSettings.EntitySets.TryGetValue("EmployeeCards", out var entitySet))
            return ApiResponse<EmployeeCard>.Failure("Entity set not configured");

        var requestUri = $"{entitySet}?$filter=No eq '{employeeNo}'";
        var response = await _navisionService.GetSingleAsync<EmployeeCard>(requestUri);

        if (!response.Successful)
            return ApiResponse<EmployeeCard>.Failure(response.Message ?? "Failed to fetch employee card");

        if (response.Data == null)
            return ApiResponse<EmployeeCard>.Failure("Employee card not found");

        return ApiResponse<EmployeeCard>.Success("Success", response.Data);
    }

    public async Task<ApiResponse<PagedResult<EmployeeCard>>> SearchEmployeeCardsAsync(EmployeeCardFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("EmployeeCards", out var entitySet))
            return ApiResponse<PagedResult<EmployeeCard>>.Failure("Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<EmployeeCard>(requestUri);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    
}