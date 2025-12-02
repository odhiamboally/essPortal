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
internal sealed class LeaveApplicationCardService : ILeaveApplicationCardService
{
    private readonly INavisionService _navisionService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LeaveApplicationCardService> _logger;
    private readonly BCSettings _bcSettings;

    public LeaveApplicationCardService(
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        INavisionService navisionService,
        ILogger<LeaveApplicationCardService> logger,
        IOptions<BCSettings> bcSettings)
    {
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _navisionService = navisionService;
        _logger = logger;
        _bcSettings = bcSettings.Value;
    }

    public async Task<ApiResponse<PagedResult<LeaveApplicationCard>>> GetLeaveApplicationCardsAsync()
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
            return ApiResponse<PagedResult<LeaveApplicationCard>>.Failure("Leave Application Cards Entity set not configured");

        var response = await _navisionService.GetMultipleAsync<LeaveApplicationCard>(entitySet);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    public async Task<ApiResponse<LeaveApplicationCard>> GetLeaveApplicationCardByNoAsync(string applicationNo)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
            return ApiResponse<LeaveApplicationCard>.Failure("Leave Application Cards Entity set not configured");

        var requestUri = $"{entitySet}?$filter=Application_No eq '{applicationNo}'";
        var response = await _navisionService.GetSingleAsync<LeaveApplicationCard>(requestUri);

        if (!response.Successful)
            return ApiResponse<LeaveApplicationCard>.Failure(response.Message ?? "Failed to fetch leave application card");

        return ApiResponse<LeaveApplicationCard>.Success("Success", response.Data ?? new());
    }

    public async Task<ApiResponse<PagedResult<Domain.Entities.LeaveApplicationCard>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
            return ApiResponse<PagedResult<Domain.Entities.LeaveApplicationCard>>.Failure("Leave Application Cards Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<Domain.Entities.LeaveApplicationCard>(requestUri);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    public async Task<ApiResponse<bool>> CreateLeaveApplicationCardAsync(LeaveApplicationCard request)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
            return ApiResponse<bool>.Failure("Leave Application Cards Entity set not configured");

        var response = await _navisionService.CreateAsync(entitySet, request);

        if (!response.Successful)
            return ApiResponse<bool>.Failure(response.Message ?? "Failed to create leave application card");

        return ApiResponse<bool>.Success("Leave application card created successfully", true);
    }

    
    
}
