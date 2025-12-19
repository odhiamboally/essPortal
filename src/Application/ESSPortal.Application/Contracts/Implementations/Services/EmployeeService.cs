using AutoMapper;

using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Domain.Enums.NavEnums;

using ESSPortal.Application.Configuration;

using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Employee;
using ESSPortal.Application.Extensions;
using ESSPortal.Application.Mappings;
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
    private readonly PaginationSetting _paginationSettings;
    private readonly IMapper _mapper;

    public EmployeeService(
        ILogger<EmployeeService> logger,
        IOptions<BCSettings> bcSettings,
        IOptions<PaginationSetting> paginationSettings,
        INavisionService navisionService,
        IMapper mapper)
    {
        _logger = logger;
        _bcSettings = bcSettings.Value;
        _paginationSettings = paginationSettings.Value;
        _navisionService = navisionService;
        _mapper = mapper;
    }

    // Employees
    public async Task<ApiResponse<PagedResult<EmployeeResponse>>> GetEmployeesAsync()
    {
        if (!_bcSettings.EntitySets.TryGetValue("Employees", out var entitySet))
            return ApiResponse<PagedResult<EmployeeResponse>>.Failure("Entity set not configured");

        var response = await _navisionService.GetMultipleAsync<Employees>(entitySet);
        var (items, rawJson) = response.Data;
        var mappedItems = items.Select(emp => emp.ToEmployeeResponse()).ToList();

        return ApiResponse<PagedResult<EmployeeResponse>>.Success("Success", new PagedResult<EmployeeResponse>
        {
            Items = mappedItems,
            Cursor = null,
            NextCursor = null,
            PageSize = mappedItems.Count,
            CurrentPage = 1,
            IsFirstPage = true,
            IsLastPage = false,
            TotalPages = (int)Math.Ceiling((double)mappedItems.Count / _paginationSettings.DefaultPageSize),
            TotalCount = mappedItems.Count
        });
    }

    public async Task<ApiResponse<EmployeeResponse>> GetEmployeeByNoAsync(string employeeNo)
    {
        if (!_bcSettings.EntitySets.TryGetValue("Employees", out var entitySet))
            return ApiResponse<EmployeeResponse>.Failure("Entity set not configured");

        var requestUri = $"{entitySet}?$filter=No eq '{employeeNo}'";
        var response = await _navisionService.GetSingleAsync<Employees>(requestUri);

        if (!response.Successful)
            return ApiResponse<EmployeeResponse>.Failure(response.Message ?? "Failed to fetch employee");

        if (response.Data == null)
            return ApiResponse<EmployeeResponse>.Failure("Employee not found");

        var employeeResponse = response.Data.ToEmployeeResponse();
        
        return ApiResponse<EmployeeResponse>.Success("Success", employeeResponse);
    }

    public async Task<ApiResponse<PagedResult<EmployeeResponse>>> SearchEmployeesAsync(EmployeesFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("Employees", out var entitySet))
            return ApiResponse<PagedResult<EmployeeResponse>>.Failure("Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<Employees>(requestUri);
        if (!response.Successful)
            return ApiResponse<PagedResult<EmployeeResponse>>.Failure(response.Message ?? "Failed to fetch employee records");

        var (items, rawJson) = response.Data;

        var mappedItems = items.Select(emp => emp.ToEmployeeResponse()).ToList();

        return ApiResponse<PagedResult<EmployeeResponse>>.Success("Success", new PagedResult<EmployeeResponse>
        {
            Items = mappedItems,
            Cursor = null,
            NextCursor = null,
            PageSize = mappedItems.Count,
            CurrentPage = 1,
            IsFirstPage = true,
            IsLastPage = false,
            TotalPages = (int)Math.Ceiling((double)mappedItems.Count / _paginationSettings.DefaultPageSize),
            TotalCount = mappedItems.Count
        });
    }

    public Task<ApiResponse<bool>> CreateEmployeeCardsAsync(CreateEmployeeCardRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<PagedResult<EmployeeCardResponse>>> GetEmployeeCardsAsync()
    {
        if (!_bcSettings.EntitySets.TryGetValue("EmployeeCards", out var entitySet))
            return ApiResponse<PagedResult<EmployeeCardResponse>>.Failure("Entity set not configured");

        var response = await _navisionService.GetMultipleAsync<EmployeeCard>(entitySet);
        if (!response.Successful)
            return ApiResponse<PagedResult<EmployeeCardResponse>>.Failure(response.Message ?? "Failed to fetch employee cards");

        var (items, rawJson) = response.Data;
        var mappedItems = items.Select(card => card.ToEmployeeCardResponse()).ToList();

        return ApiResponse<PagedResult<EmployeeCardResponse>>.Success("Success", new PagedResult<EmployeeCardResponse>
        {
            Items = mappedItems,
            Cursor = null,
            NextCursor = null,
            PageSize = mappedItems.Count,
            CurrentPage = 1,
            IsFirstPage = true,
            IsLastPage = false,
            TotalPages = (int)Math.Ceiling((double)mappedItems.Count / _paginationSettings.DefaultPageSize),
            TotalCount = mappedItems.Count
        });

    }

    public async Task<ApiResponse<EmployeeCardResponse>> GetEmployeeCardByNoAsync(string employeeNo)
    {
        if (!_bcSettings.EntitySets.TryGetValue("EmployeeCards", out var entitySet))
            return ApiResponse<EmployeeCardResponse>.Failure("Entity set not configured");

        var requestUri = $"{entitySet}?$filter=No eq '{employeeNo}'";
        var response = await _navisionService.GetSingleAsync<EmployeeCard>(requestUri);

        if (!response.Successful)
            return ApiResponse<EmployeeCardResponse>.Failure(response.Message ?? "Failed to fetch employee card");

        if (response.Data == null)
            return ApiResponse<EmployeeCardResponse>.Failure("Employee card not found");

        var employeeCardResponse = response.Data.ToEmployeeCardResponse();

        return ApiResponse<EmployeeCardResponse>.Success("Success", employeeCardResponse);
    }

    public async Task<ApiResponse<PagedResult<EmployeeCardResponse>>> SearchEmployeeCardsAsync(EmployeeCardFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("EmployeeCards", out var entitySet))
            return ApiResponse<PagedResult<EmployeeCardResponse>>.Failure("Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<EmployeeCard>(requestUri);
        if (!response.Successful)
            return ApiResponse<PagedResult<EmployeeCardResponse>>.Failure(response.Message ?? "Failed to fetch employee card records");

        var (items, rawJson) = response.Data;
        var mappedItems = items.Select(card => card.ToEmployeeCardResponse()).ToList();

        return ApiResponse<PagedResult<EmployeeCardResponse>>.Success("Success", new PagedResult<EmployeeCardResponse>
        {
            Items = mappedItems,
            Cursor = null,
            NextCursor = null,
            PageSize = mappedItems.Count,
            CurrentPage = 1,
            IsFirstPage = true,
            IsLastPage = false,
            TotalPages = (int)Math.Ceiling((double)mappedItems.Count / _paginationSettings.DefaultPageSize),
            TotalCount = mappedItems.Count
        });

    }

    
}