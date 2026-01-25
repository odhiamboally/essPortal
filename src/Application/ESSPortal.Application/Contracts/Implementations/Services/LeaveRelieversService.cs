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
using ESSPortal.Domain.NavEntities.LeaveApplication;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class LeaveRelieversService(
    ICacheService cacheService,
    IUnitOfWork unitOfWork,
    INavisionService navisionService,
    ILogger<LeaveRelieversService> logger,
    IOptions<BCSettings> bcSettings) : ILeaveRelieversService
{
    private readonly INavisionService _navisionService = navisionService;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<LeaveRelieversService> _logger = logger;
    private readonly BCSettings _bcSettings = bcSettings.Value;

    // Read operations
    public async Task<AppResponse<PagedResult<LeaveRelieverResponse>>> GetLeaveRelieversAsync()
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveRelievers", out var entitySet))
                return AppResponse<PagedResult<LeaveRelieverResponse>>.Failure("Leave Relievers Entity set not configured");

            var response = await _navisionService.GetMultipleAsync<LeaveRelievers>(entitySet);
            if (!response.Successful)
                return AppResponse<PagedResult<LeaveRelieverResponse>>.Failure(response.Message ?? "Failed to fetch leave relievers");

            var (items, rawJson) = response.Data;

            if (items == null || !items.Any())
                return AppResponse<PagedResult<LeaveRelieverResponse>>.Success("No leave relievers found", new());

            var mappedItems = items.ToLeaveRelieverResponses().ToList();

            return AppResponse<PagedResult<LeaveRelieverResponse>>.Success("Success", new PagedResult<LeaveRelieverResponse>
            {
                Items = mappedItems,
                TotalCount = mappedItems.Count
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching LeaveRelievers: {Message}", ex.Message);
            throw;
        }

    }

    public async Task<AppResponse<LeaveRelieverResponse>> GetLeaveRelieverAsync(string leaveCode, string staffNo)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveRelievers", out var entitySet))
                return AppResponse<LeaveRelieverResponse>.Failure("Leave Relievers Entity set not configured");

            var requestUri = $"{entitySet}?$filter=Leave_Code eq '{leaveCode}' and Staff_No eq '{staffNo}'";
            var response = await _navisionService.GetSingleAsync<LeaveRelievers>(requestUri);

            if (!response.Successful)
                return AppResponse<LeaveRelieverResponse>.Failure(response.Message ?? "Failed to fetch leave reliever");

            var mappedItems = response.Data?.ToLeaveRelieverResponse();

            return AppResponse<LeaveRelieverResponse>.Success("Success", mappedItems ?? new());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching LeaveReliever: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<AppResponse<PagedResult<LeaveRelieverResponse>>> GetLeaveRelieversByApplicationNoAsync(string applicationNo)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveRelievers", out var entitySet))
                return AppResponse<PagedResult<LeaveRelieverResponse>>.Failure("Leave Relievers Entity set not configured");

            var requestUri = $"{entitySet}?$filter=Application_No eq '{applicationNo}'";
            var response = await _navisionService.GetMultipleAsync<LeaveRelievers>(requestUri);

            if (!response.Successful)
                return AppResponse<PagedResult<LeaveRelieverResponse>>.Failure(response.Message ?? "Failed to fetch leave relievers");

            var (items, rawJson) = response.Data;
            if (items == null || !items.Any())
                return AppResponse<PagedResult<LeaveRelieverResponse>>.Success("No leave relievers found", new());

            var mappedItems = items.ToLeaveRelieverResponses().ToList();

            return AppResponse<PagedResult<LeaveRelieverResponse>>.Success("Success", new PagedResult<LeaveRelieverResponse>
            {
                Items = mappedItems,
                TotalCount = mappedItems.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching LeaveRelievers by ApplicationNo: {Message}", ex.Message);
            throw;
        }


    }

    public async Task<AppResponse<PagedResult<LeaveRelieverResponse>>> SearchLeaveRelieversAsync(LeaveRelieverFilter filter)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveRelievers", out var entitySet))
                return AppResponse<PagedResult<LeaveRelieverResponse>>.Failure("Leave Relievers Entity set not configured");

            var odataQuery = filter.BuildODataFilter();
            var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

            var response = await _navisionService.GetMultipleAsync<LeaveRelievers>(requestUri);
            if (!response.Successful)
                return AppResponse<PagedResult<LeaveRelieverResponse>>.Failure(response.Message ?? "Failed to fetch leave relievers");

            var (items, rawJson) = response.Data;

            if (items == null || !items.Any())
                return AppResponse<PagedResult<LeaveRelieverResponse>>.Success("No leave relievers found", new());

            var mappedItems = items.ToLeaveRelieverResponses().ToList();

            return AppResponse<PagedResult<LeaveRelieverResponse>>.Success("Success", new PagedResult<LeaveRelieverResponse>
            {
                Items = mappedItems,
                TotalCount = mappedItems.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching LeaveRelievers: {Message}", ex.Message);
            throw;
        }

    }

    public async Task<AppResponse<bool>> CreateAsync(CreateLeaveRelieverRequest request)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("CreateLeaveReliever", out var entitySet))
                return AppResponse<bool>.Failure("Leave Relievers Entity set not configured");

            var leaveReliever = request.ToLeaveRelievers();

            var response = await _navisionService.CreateAsync(entitySet, leaveReliever);
            if (!response.Successful)
                return AppResponse<bool>.Failure(response.Message ?? "Failed to create record.");

            return AppResponse<bool>.Success(response.Message ?? "Success", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating LeaveReliever: {Message}", ex.Message);
            throw;
        }

    }

    public async Task<AppResponse<bool>> CreateMultipleAsync(List<CreateLeaveRelieverRequest> requests)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("CreateLeaveRelievers", out var entitySet))
                return AppResponse<bool>.Failure("Leave Relievers Entity set not configured");

            var leaveRelievers = requests.Select(r => r.ToLeaveRelievers()).ToList();

            var response = await _navisionService.CreateMultipleAsync(entitySet, leaveRelievers);
            if (!response.Successful)
                return AppResponse<bool>.Failure(response.Message ?? "Failed to create multiple records.");

            return AppResponse<bool>.Success(response.Message ?? "Success", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating multiple LeaveRelievers: {Message}", ex.Message);
            throw;
        }

    }

}
