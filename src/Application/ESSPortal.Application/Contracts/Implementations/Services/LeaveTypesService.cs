using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;

using ESSPortal.Application.Extensions;
using ESSPortal.Application.Mappings;
using ESSPortal.Application.Utilities;
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.NavEntities;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

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

    public async Task<AppResponse<bool>> CreateLeaveTypeAsync(CreateLeaveTypeRequest request)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveTypes", out var entitySet))
            return AppResponse<bool>.Failure("Leave Types Entity set not configured");

        var response = await _navisionService.CreateAsync(entitySet, request);

        return response.Successful
            ? AppResponse<bool>.Success("Leave type created successfully", true)
            : AppResponse<bool>.Failure(response.Message ?? "Failed to create leave type");
    }

    public async Task<AppResponse<PagedResult<LeaveTypeResponse>>> GetLeaveTypesAsync()
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveTypes", out var entitySet))
                return AppResponse<PagedResult<LeaveTypeResponse>>.Failure("Leave Types Entity set not configured");

            var response = await _navisionService.GetMultipleAsync<LeaveTypes>(entitySet);
            if (!response.Successful)
                return AppResponse<PagedResult<LeaveTypeResponse>>.Failure(response.Message ?? "Failed to fetch leave types");

            var (items, _) = response.Data;

            var mappedItems = items.ToLeaveTypeResponses();

            return AppResponse<PagedResult<LeaveTypeResponse>>.Success("Success", new PagedResult<LeaveTypeResponse>
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

    public async Task<AppResponse<LeaveTypeResponse>> GetLeaveTypeByCodeAsync(string code)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveTypes", out var entitySet))
                return AppResponse<LeaveTypeResponse>.Failure("Leave Types Entity set not configured");

            var requestUri = $"{entitySet}?$filter=Code eq '{code}'";
            var response = await _navisionService.GetSingleAsync<LeaveTypes>(requestUri);

            if (!response.Successful)
                return AppResponse<LeaveTypeResponse>.Failure(response.Message ?? "Failed to fetch leave type");

            if (response.Data == null)
                return AppResponse<LeaveTypeResponse>.Failure("Leave type not found");

            var leaveTypeResponse = LeaveMappingExtensions.ToLeaveTypeResponse(response.Data);

            return AppResponse<LeaveTypeResponse>.Success("Success", leaveTypeResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching leave type by code");

            throw;
        }
    }

    public async Task<AppResponse<PagedResult<LeaveTypeResponse>>> SearchLeaveTypesAsync(LeaveTypeFilter filter)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveTypes", out var entitySet))
                return AppResponse<PagedResult<LeaveTypeResponse>>.Failure("Leave Types Entity set not configured");

            var odataQuery = filter.BuildODataFilter();
            var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

            var response = await _navisionService.GetMultipleAsync<LeaveTypes>(requestUri);

            if (!response.Successful)
                return AppResponse<PagedResult<LeaveTypeResponse>>.Failure(response.Message ?? "Failed to fetch leave types");

            var (items, _) = response.Data;

            var mappedItems = items.ToLeaveTypeResponses();

            return AppResponse<PagedResult<LeaveTypeResponse>>.Success("Success", new PagedResult<LeaveTypeResponse>
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
