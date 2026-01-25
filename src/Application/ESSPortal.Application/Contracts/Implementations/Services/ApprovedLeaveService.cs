using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;

using ESSPortal.Application.Extensions;
using ESSPortal.Application.Utilities;
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.NavEntities;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.ModelFilters;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace ESSPortal.Application.Contracts.Implementations.Services;

internal sealed class ApprovedLeaveService : IApprovedLeaveService
{
    private readonly INavisionService _navisionService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LeaveApplicationCardService> _logger;
    private readonly BCSettings _bcSettings;

    public ApprovedLeaveService(
        INavisionService navisionService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        ILogger<LeaveApplicationCardService> logger,
        IOptions<BCSettings> bcSettings)
    {
        _navisionService = navisionService;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _bcSettings = bcSettings.Value;
    }

    public async Task<AppResponse<PagedResult<ApprovedLeaves>>> SearchLeaveApplicationCardsAsync(ApprovedLeaveFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("ApprovedLeaves", out var entitySet))
            return AppResponse<PagedResult<ApprovedLeaves>>.Failure("Leave Application Cards Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<ApprovedLeaves>(requestUri);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    

}
