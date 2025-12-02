using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Extensions;
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

    // Read operations
    public async Task<ApiResponse<PagedResult<LeaveApplicationList>>> GetLeaveApplicationListsAsync()
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationLists", out var entitySet))
            return ApiResponse<PagedResult<LeaveApplicationList>>.Failure("Leave Application Lists Entity set not configured");

        var response = await _navisionService.GetMultipleAsync<LeaveApplicationList>(entitySet);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    public async Task<ApiResponse<LeaveApplicationList>> GetLeaveApplicationListByNoAsync(string applicationNo)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationLists", out var entitySet))
            return ApiResponse<LeaveApplicationList>.Failure("Leave Application Lists Entity set not configured");

        var requestUri = $"{entitySet}?$filter=Application_No eq '{applicationNo}'";
        var response = await _navisionService.GetSingleAsync<LeaveApplicationList>(requestUri);

        if (!response.Successful)
            return ApiResponse<LeaveApplicationList>.Failure(response.Message ?? "Failed to fetch leave application list");

        return ApiResponse<LeaveApplicationList>.Success("Success", response.Data ?? new());
    }

    public async Task<ApiResponse<PagedResult<LeaveApplicationList>>> SearchLeaveApplicationListsAsync(LeaveApplicationListFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationLists", out var entitySet))
            return ApiResponse<PagedResult<LeaveApplicationList>>.Failure("Leave Application Lists Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<LeaveApplicationList>(requestUri);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    

    
}
