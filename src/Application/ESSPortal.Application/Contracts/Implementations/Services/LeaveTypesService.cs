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

    // Read operations
    public async Task<ApiResponse<PagedResult<LeaveTypes>>> GetLeaveTypesAsync()
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveTypes", out var entitySet))
            return ApiResponse<PagedResult<LeaveTypes>>.Failure("Leave Types Entity set not configured");

        var response = await _navisionService.GetMultipleAsync<LeaveTypes>(entitySet);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    public async Task<ApiResponse<LeaveTypes>> GetLeaveTypeByCodeAsync(string code)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveTypes", out var entitySet))
            return ApiResponse<LeaveTypes>.Failure("Leave Types Entity set not configured");

        var requestUri = $"{entitySet}?$filter=Code eq '{code}'";
        var response = await _navisionService.GetSingleAsync<LeaveTypes>(requestUri);

        if (!response.Successful)
            return ApiResponse<LeaveTypes>.Failure(response.Message ?? "Failed to fetch leave type");

        return ApiResponse<LeaveTypes>.Success("Success", response.Data ?? new());
    }

    public async Task<ApiResponse<PagedResult<LeaveTypes>>> SearchLeaveTypesAsync(LeaveTypeFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveTypes", out var entitySet))
            return ApiResponse<PagedResult<LeaveTypes>>.Failure("Leave Types Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<LeaveTypes>(requestUri);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

}
