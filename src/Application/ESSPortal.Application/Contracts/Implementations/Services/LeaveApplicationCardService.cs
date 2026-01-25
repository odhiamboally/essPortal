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

    public async Task<AppResponse<PagedResult<LeaveApplicationCardResponse>>> GetLeaveApplicationCardsAsync()
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
                return AppResponse<PagedResult<LeaveApplicationCardResponse>>.Failure("Leave Application Cards Entity set not configured");

            var response = await _navisionService.GetMultipleAsync<Domain.Entities.LeaveApplicationCard>(entitySet);

            if (!response.Successful)
                return AppResponse<PagedResult<LeaveApplicationCardResponse>>.Failure(response.Message ?? "Failed to fetch leave application cards");

            var (items, rawJson) = response.Data;

            var mappedItems = items.Select(item => item.ToLeaveApplicationCardResponseExtended()).ToList();

            return AppResponse<PagedResult<LeaveApplicationCardResponse>>.Success("Success", new PagedResult<LeaveApplicationCardResponse>
            {
                Items = mappedItems,
                TotalCount = mappedItems.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching leave application cards");

            throw;
        }

    }

    public async Task<AppResponse<LeaveApplicationCardResponse>> GetLeaveApplicationCardByNoAsync(string applicationNo)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
                return AppResponse<LeaveApplicationCardResponse>.Failure("Leave Application Cards Entity set not configured");

            var requestUri = $"{entitySet}?$filter=Application_No eq '{applicationNo}'";
            var response = await _navisionService.GetSingleAsync<Domain.Entities.LeaveApplicationCard>(requestUri);

            if (!response.Successful)
                return AppResponse<LeaveApplicationCardResponse>.Failure(response.Message ?? "Failed to fetch leave application card");

            var mappedData = response.Data?.ToLeaveApplicationCardResponseExtended();
            return AppResponse<LeaveApplicationCardResponse>.Success("Success", mappedData ?? new());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching leave application card by number");

            throw;
        }

    }

    public async Task<AppResponse<PagedResult<LeaveApplicationCardResponse>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
                return AppResponse<PagedResult<LeaveApplicationCardResponse>>.Failure("Leave Application Cards Entity set not configured");

            var odataQuery = filter.BuildODataFilter();
            var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

            var response = await _navisionService.GetMultipleAsync<Domain.Entities.LeaveApplicationCard>(requestUri);
            if (!response.Successful)
                return AppResponse<PagedResult<LeaveApplicationCardResponse>>.Failure(response.Message ?? "Failed to fetch leave application cards");

            var (items, rawJson) = response.Data;

            var mappedItems = items.Select(item => item.ToLeaveApplicationCardResponseExtended()).ToList();

            return AppResponse<PagedResult<LeaveApplicationCardResponse>>.Success("Success", new PagedResult<LeaveApplicationCardResponse>
            {
                Items = mappedItems,
                TotalCount = mappedItems.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching leave application cards");

            throw;
        }

    }

    public async Task<AppResponse<bool>> CreateLeaveApplicationCardAsync(CreateLeaveApplicationCardRequest request)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
                return AppResponse<bool>.Failure("Leave Application Cards Entity set not configured");

            var response = await _navisionService.CreateAsync(entitySet, request);

            if (!response.Successful)
                return AppResponse<bool>.Failure(response.Message ?? "Failed to create leave application card");

            return AppResponse<bool>.Success("Leave application card created successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave application card");

            throw;
        }
    }

    
    
}
