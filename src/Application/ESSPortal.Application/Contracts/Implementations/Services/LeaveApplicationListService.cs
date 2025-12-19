using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Application.Extensions;
using ESSPortal.Application.Mappings;
using ESSPortal.Application.Utilities;
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.NavEntities;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class LeaveApplicationListService : ILeaveApplicationListService
{
    private readonly INavisionService _navisionService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LeaveApplicationListService> _logger;
    private readonly BCSettings _bcSettings;

    public LeaveApplicationListService(
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        INavisionService navisionService,
        ILogger<LeaveApplicationListService> logger,
        IOptions<BCSettings> bcSettings)
    {
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _navisionService = navisionService;
        _logger = logger;
        _bcSettings = bcSettings.Value;
    }

    public Task<ApiResponse<bool>> CreateLeaveApplicationListAsync(CreateLeaveApplicationListRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<PagedResult<LeaveApplicationListResponse>>> GetLeaveApplicationListsAsync()
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationLists", out var entitySet))
            return ApiResponse<PagedResult<LeaveApplicationListResponse>>.Failure("Leave Application Lists Entity set not configured");

        var response = await _navisionService.GetMultipleAsync<LeaveApplicationList>(entitySet);
        if (!response.Successful)
            return ApiResponse<PagedResult<LeaveApplicationListResponse>>.Failure(response.Message ?? "Failed to fetch leave application lists");

        var (items, rawJson) = response.Data;
        var mappedItems = items.ToLeaveApplicationListResponses().ToList();
        return ApiResponse<PagedResult<LeaveApplicationListResponse>>.Success("Success", new PagedResult<LeaveApplicationListResponse>
        {
            Items = mappedItems,
            TotalCount = mappedItems.Count,
        });

    }

    public async Task<ApiResponse<LeaveApplicationListResponse?>> GetLeaveApplicationListByNoAsync(string applicationNo)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationLists", out var entitySet))
            return ApiResponse<LeaveApplicationListResponse?>.Failure("Leave Application Lists Entity set not configured");

        var requestUri = $"{entitySet}?$filter=Application_No eq '{applicationNo}'";
        var response = await _navisionService.GetSingleAsync<LeaveApplicationList>(requestUri);

        if (!response.Successful)
            return ApiResponse<LeaveApplicationListResponse?>.Failure(response.Message ?? "Failed to fetch leave application list");

        var mappedItem = response.Data?.ToLeaveApplicationListResponse();

        return ApiResponse<LeaveApplicationListResponse?>.Success("Success", mappedItem ?? new());
    }

    public async Task<ApiResponse<PagedResult<LeaveApplicationListResponse>>> SearchLeaveApplicationListsAsync(LeaveApplicationListFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationLists", out var entitySet))
            return ApiResponse<PagedResult<LeaveApplicationListResponse>>.Failure("Leave Application Lists Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<LeaveApplicationList>(requestUri);

        if (!response.Successful)
            return ApiResponse<PagedResult<LeaveApplicationListResponse>>.Failure(response.Message ?? "Failed to search leave application lists");

        var (items, rawJson) = response.Data;

        var mappedItems = items.ToLeaveApplicationListResponses().ToList();

        return ApiResponse<PagedResult<LeaveApplicationListResponse>>.Success("Success", new PagedResult<LeaveApplicationListResponse>
        {
            Items = mappedItems,
            TotalCount = mappedItems.Count,
        });
    }

   
}
