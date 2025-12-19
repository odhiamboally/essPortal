using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Employee;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Application.Extensions;
using ESSPortal.Application.Mappings;
using ESSPortal.Application.Utilities;
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.NavEntities;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Linq;
using System.Text.Json;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class LeaveTypesService : ILeaveTypesService
{
    private readonly INavisionService _navisionService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LeaveTypesService> _logger;
    private readonly BCSettings _bcSettings;

    public LeaveTypesService(
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        INavisionService navisionService,
        ILogger<LeaveTypesService> logger,
        IOptions<BCSettings> bcSettings)
    {
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _navisionService = navisionService;
        _logger = logger;
        _bcSettings = bcSettings.Value;
    }

    public async Task<ApiResponse<bool>> CreateLeaveTypeAsync(CreateLeaveTypeRequest request)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveTypes", out var entitySet))
            return ApiResponse<bool>.Failure("Leave Types Entity set not configured");

        var response = await _navisionService.CreateAsync(entitySet, request);

        return response.Successful
            ? ApiResponse<bool>.Success("Leave type created successfully", true)
            : ApiResponse<bool>.Failure(response.Message ?? "Failed to create leave type");
    }

    public async Task<ApiResponse<PagedResult<LeaveTypeResponse>>> GetLeaveTypesAsync()
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveTypes", out var entitySet))
                return ApiResponse<PagedResult<LeaveTypeResponse>>.Failure("Leave Types Entity set not configured");

            var response = await _navisionService.GetMultipleAsync<LeaveTypes>(entitySet);
            if (!response.Successful)
                return ApiResponse<PagedResult<LeaveTypeResponse>>.Failure(response.Message ?? "Failed to fetch leave types");

            var (items, _) = response.Data;

            var mappedItems = items.ToLeaveTypeResponses();

            return ApiResponse<PagedResult<LeaveTypeResponse>>.Success("Success", new PagedResult<LeaveTypeResponse>
            {
                Items = mappedItems.ToList(),
                Cursor = null,
                NextCursor = null,
                PageSize = mappedItems.Count(),
                CurrentPage = 1,
                IsFirstPage = true,
                IsLastPage = false,
                TotalCount = mappedItems.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching leave types");

            throw;
        }
    }

    public async Task<ApiResponse<LeaveTypeResponse>> GetLeaveTypeByCodeAsync(string code)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveTypes", out var entitySet))
                return ApiResponse<LeaveTypeResponse>.Failure("Leave Types Entity set not configured");

            var requestUri = $"{entitySet}?$filter=Code eq '{code}'";
            var response = await _navisionService.GetSingleAsync<LeaveTypes>(requestUri);

            if (!response.Successful)
                return ApiResponse<LeaveTypeResponse>.Failure(response.Message ?? "Failed to fetch leave type");

            if (response.Data == null)
                return ApiResponse<LeaveTypeResponse>.Failure("Leave type not found");

            var leaveTypeResponse = LeaveMappingExtensions.ToLeaveTypeResponse(response.Data);

            return ApiResponse<LeaveTypeResponse>.Success("Success", leaveTypeResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching leave type by code");

            throw;
        }
    }

    public async Task<ApiResponse<PagedResult<LeaveTypeResponse>>> SearchLeaveTypesAsync(LeaveTypeFilter filter)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveTypes", out var entitySet))
                return ApiResponse<PagedResult<LeaveTypeResponse>>.Failure("Leave Types Entity set not configured");

            var odataQuery = filter.BuildODataFilter();
            var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

            var response = await _navisionService.GetMultipleAsync<LeaveTypes>(requestUri);

            if (!response.Successful)
                return ApiResponse<PagedResult<LeaveTypeResponse>>.Failure(response.Message ?? "Failed to fetch leave types");

            var (items, _) = response.Data;

            var mappedItems = items.ToLeaveTypeResponses();

            return ApiResponse<PagedResult<LeaveTypeResponse>>.Success("Success", new PagedResult<LeaveTypeResponse>
            {
                Items = mappedItems.ToList(),
                Cursor = null,
                NextCursor = null,
                PageSize = mappedItems.Count(),
                CurrentPage = 1,
                IsFirstPage = true,
                IsLastPage = false,
                TotalCount = mappedItems.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching leave types");
            throw;
        }

    }

    
}
