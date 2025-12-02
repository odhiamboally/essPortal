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
internal sealed class LeaveStatisticsFactboxService : ILeaveStatisticsFactboxService
{
    private readonly INavisionService _navisionService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LeaveStatisticsFactboxService> _logger;
    private readonly BCSettings _bcSettings;

    public LeaveStatisticsFactboxService(
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        INavisionService navisionService,
        ILogger<LeaveStatisticsFactboxService> logger,
        IOptions<BCSettings> bcSettings)
    {
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _navisionService = navisionService;
        _logger = logger;
        _bcSettings = bcSettings.Value;
    }

    // Read operations
    public async Task<ApiResponse<PagedResult<LeaveStatisticsFactbox>>> GetLeaveStatisticsAsync()
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveStatisticsFactbox", out var entitySet))
            return ApiResponse<PagedResult<LeaveStatisticsFactbox>>.Failure("Leave Statistics Factbox Entity set not configured");

        var response = await _navisionService.GetMultipleAsync<LeaveStatisticsFactbox>(entitySet);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    public async Task<ApiResponse<PagedResult<LeaveStatisticsFactbox>>> SearchLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveStatisticsFactbox", out var entitySet))
            return ApiResponse<PagedResult<LeaveStatisticsFactbox>>.Failure("Leave Statistics Factbox Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<LeaveStatisticsFactbox>(requestUri);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }
    
}
