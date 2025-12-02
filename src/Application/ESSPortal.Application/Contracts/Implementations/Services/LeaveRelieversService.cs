using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Extensions;
using ESSPortal.Application.Utilities;
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.NavEntities;
using ESSPortal.Domain.NavEntities.LeaveApplication;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class LeaveRelieversService : ILeaveRelieversService
{
    private readonly INavisionService _navisionService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LeaveRelieversService> _logger;
    private readonly BCSettings _bcSettings;

    public LeaveRelieversService(
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        INavisionService navisionService,
        ILogger<LeaveRelieversService> logger,
        IOptions<BCSettings> bcSettings)
    {
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _navisionService = navisionService;
        _logger = logger;
        _bcSettings = bcSettings.Value;
    }

    // Read operations
    public async Task<ApiResponse<PagedResult<LeaveRelievers>>> GetLeaveRelieversAsync()
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveRelievers", out var entitySet))
            return ApiResponse<PagedResult<LeaveRelievers>>.Failure("Leave Relievers Entity set not configured");

        var response = await _navisionService.GetMultipleAsync<LeaveRelievers>(entitySet);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    public async Task<ApiResponse<LeaveRelievers>> GetLeaveRelieverAsync(string leaveCode, string staffNo)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveRelievers", out var entitySet))
            return ApiResponse<LeaveRelievers>.Failure("Leave Relievers Entity set not configured");

        var requestUri = $"{entitySet}?$filter=Leave_Code eq '{leaveCode}' and Staff_No eq '{staffNo}'";
        var response = await _navisionService.GetSingleAsync<LeaveRelievers>(requestUri);

        if (!response.Successful)
            return ApiResponse<LeaveRelievers>.Failure(response.Message ?? "Failed to fetch leave reliever");

        return ApiResponse<LeaveRelievers>.Success("Success", response.Data ?? new());
    }

    public async Task<ApiResponse<List<LeaveRelievers>>> GetLeaveRelieversByApplicationNoAsync(string applicationNo)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveRelievers", out var entitySet))
            return ApiResponse<List<LeaveRelievers>>.Failure("Leave Relievers Entity set not configured");

        var requestUri = $"{entitySet}?$filter=Application_No eq '{applicationNo}'";
        var response = await _navisionService.GetMultipleAsync<LeaveRelievers>(requestUri);

        if (!response.Successful)
            return ApiResponse<List<LeaveRelievers>>.Failure(response.Message ?? "Failed to fetch leave relievers");

        var (items, rawJson) = response.Data;
        if (items == null || !items.Any())
            return ApiResponse<List<LeaveRelievers>>.Success("No leave relievers found", new List<LeaveRelievers>());

        // Deserialize the raw JSON to PagedResult
        JsonSerializer.Deserialize<PagedResult<LeaveRelievers>>(rawJson);

        return ApiResponse<List<LeaveRelievers>>.Success("Success", items);
    }

    public async Task<ApiResponse<PagedResult<LeaveRelievers>>> SearchLeaveRelieversAsync(LeaveRelieversFilter filter)
    {
        if (!_bcSettings.EntitySets.TryGetValue("LeaveRelievers", out var entitySet))
            return ApiResponse<PagedResult<LeaveRelievers>>.Failure("Leave Relievers Entity set not configured");

        var odataQuery = filter.BuildODataFilter();
        var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

        var response = await _navisionService.GetMultipleAsync<LeaveRelievers>(requestUri);
        return await NavisionResponseHandler.HandlePagedResponse(response);
    }

    public async Task<ApiResponse<bool>> CreateAsync(LeaveReliever leaveReliever)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("CreateLeaveReliever", out var entitySet))
                return ApiResponse<bool>.Failure("Leave Relievers Entity set not configured");

            var response = await _navisionService.CreateAsync(entitySet, leaveReliever);
            if (!response.Successful)
                return ApiResponse<bool>.Failure(response.Message ?? "Failed to create record.");
            return ApiResponse<bool>.Success(response.Message ?? "Success", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating LeaveReliever: {Message}", ex.Message);
            return ApiResponse<bool>.Failure("An error occurred while creating the record.");
        }

    }

    public async Task<ApiResponse<bool>> CreateAsync(LeaveRelievers leaveRelievers)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("CreateLeaveReliever", out var entitySet))
                return ApiResponse<bool>.Failure("Leave Relievers Entity set not configured");

            var response = await _navisionService.CreateAsync(entitySet, leaveRelievers);
            if (!response.Successful)
                return ApiResponse<bool>.Failure(response.Message ?? "Failed to create record.");
            return ApiResponse<bool>.Success(response.Message ?? "Success", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating LeaveReliever: {Message}", ex.Message);
            return ApiResponse<bool>.Failure("An error occurred while creating the record.");
        }

    }

    public async Task<ApiResponse<bool>> CreateMultipleAsync(List<LeaveRelievers> leaveRelievers)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("CreateLeaveRelievers", out var entitySet))
                return ApiResponse<bool>.Failure("Leave Relievers Entity set not configured");

            var response = await _navisionService.CreateMultipleAsync(entitySet, leaveRelievers);
            if (!response.Successful)
                return ApiResponse<bool>.Failure(response.Message ?? "Failed to create multiple records.");
            return ApiResponse<bool>.Success(response.Message ?? "Success", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating multiple LeaveRelievers: {Message}", ex.Message);
            return ApiResponse<bool>.Failure("An error occurred while creating multiple records.");
        }

    }

}
