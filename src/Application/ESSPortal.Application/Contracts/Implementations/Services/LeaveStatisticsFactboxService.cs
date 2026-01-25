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
    public async Task<AppResponse<PagedResult<LeaveStatisticsFactboxResponse>>> GetLeaveStatisticsAsync()
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveStatisticsFactbox", out var entitySet))
            return AppResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Failure("Leave Statistics Factbox Entity set not configured");

        var response = await _navisionService.GetMultipleAsync<LeaveStatisticsFactbox>(entitySet);
        if (!response.Successful)
            return AppResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Failure(response.Message ?? "Failed to fetch leave statistics factbox data");

        var (items, _) = response.Data;
        if (items == null || !items.Any())
            return AppResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Success("No leave statistics found", new PagedResult<LeaveStatisticsFactboxResponse>
            {
                Items = [],
                TotalCount = 0,
                PageSize = 0,
                CurrentPage = 0,
                TotalPages = 0,
                IsFirstPage = true,
                IsLastPage = true
            });

        var mappedItems = items.ToLeaveStatisticsFactboxResponses();

        return AppResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Success("Leave statistics fetched successfully", new PagedResult<LeaveStatisticsFactboxResponse>
        {
            Items = mappedItems.ToList(),
            TotalCount = mappedItems.Count()

        });
    }

    public async Task<AppResponse<PagedResult<LeaveStatisticsFactboxResponse>>> SearchLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveStatisticsFactbox", out var entitySet))
            return AppResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Failure("Leave Statistics Factbox Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<LeaveStatisticsFactbox>(requestUri);

        if (!response.Successful)
            return AppResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Failure(response.Message ?? "Failed to fetch leave statistics factbox data");

        var (items, _) = response.Data;
        if (items == null || !items.Any())
            return AppResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Success("No leave statistics found", new PagedResult<LeaveStatisticsFactboxResponse>
            {
                Items = [],
                TotalCount = 0,
                PageSize = 0,
                CurrentPage = 0,
                TotalPages = 0,
                IsFirstPage = true,
                IsLastPage = true
            });

        var mappedItems = items.ToLeaveStatisticsFactboxResponses();

        return AppResponse<PagedResult<LeaveStatisticsFactboxResponse>>.Success("Leave statistics fetched successfully", new PagedResult<LeaveStatisticsFactboxResponse>
        {
            Items = mappedItems.ToList(),
            TotalCount = mappedItems.Count()

        });

    }
    
}
