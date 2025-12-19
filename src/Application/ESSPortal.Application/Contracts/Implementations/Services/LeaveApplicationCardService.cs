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

    public async Task<ApiResponse<PagedResult<LeaveApplicationCardResponse>>> GetLeaveApplicationCardsAsync()
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
                return ApiResponse<PagedResult<LeaveApplicationCardResponse>>.Failure("Leave Application Cards Entity set not configured");

            var response = await _navisionService.GetMultipleAsync<Domain.Entities.LeaveApplicationCard>(entitySet);

            if (!response.Successful)
                return ApiResponse<PagedResult<LeaveApplicationCardResponse>>.Failure(response.Message ?? "Failed to fetch leave application cards");

            var (items, rawJson) = response.Data;

            var mappedItems = items.Select(item => item.ToLeaveApplicationCardResponseExtended()).ToList();

            return ApiResponse<PagedResult<LeaveApplicationCardResponse>>.Success("Success", new PagedResult<LeaveApplicationCardResponse>
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

    public async Task<ApiResponse<LeaveApplicationCardResponse>> GetLeaveApplicationCardByNoAsync(string applicationNo)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
                return ApiResponse<LeaveApplicationCardResponse>.Failure("Leave Application Cards Entity set not configured");

            var requestUri = $"{entitySet}?$filter=Application_No eq '{applicationNo}'";
            var response = await _navisionService.GetSingleAsync<Domain.Entities.LeaveApplicationCard>(requestUri);

            if (!response.Successful)
                return ApiResponse<LeaveApplicationCardResponse>.Failure(response.Message ?? "Failed to fetch leave application card");

            var mappedData = response.Data?.ToLeaveApplicationCardResponseExtended();
            return ApiResponse<LeaveApplicationCardResponse>.Success("Success", mappedData ?? new());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching leave application card by number");

            throw;
        }

    }

    public async Task<ApiResponse<PagedResult<LeaveApplicationCardResponse>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
                return ApiResponse<PagedResult<LeaveApplicationCardResponse>>.Failure("Leave Application Cards Entity set not configured");

            var odataQuery = filter.BuildODataFilter();
            var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

            var response = await _navisionService.GetMultipleAsync<Domain.Entities.LeaveApplicationCard>(requestUri);
            if (!response.Successful)
                return ApiResponse<PagedResult<LeaveApplicationCardResponse>>.Failure(response.Message ?? "Failed to fetch leave application cards");

            var (items, rawJson) = response.Data;

            var mappedItems = items.Select(item => item.ToLeaveApplicationCardResponseExtended()).ToList();

            return ApiResponse<PagedResult<LeaveApplicationCardResponse>>.Success("Success", new PagedResult<LeaveApplicationCardResponse>
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

    public async Task<ApiResponse<bool>> CreateLeaveApplicationCardAsync(CreateLeaveApplicationCardRequest request)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationCards", out var entitySet))
                return ApiResponse<bool>.Failure("Leave Application Cards Entity set not configured");

            var response = await _navisionService.CreateAsync(entitySet, request);

            if (!response.Successful)
                return ApiResponse<bool>.Failure(response.Message ?? "Failed to create leave application card");

            return ApiResponse<bool>.Success("Leave application card created successfully", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave application card");

            throw;
        }
    }

    
    
}
