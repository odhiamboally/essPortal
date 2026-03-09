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
internal sealed class LeaveService : ILeaveService
{
    private readonly INavisionService _navisionService;
    private readonly ILeaveApplicationCardService _leaveApplicationCardService;
    private readonly ILeaveApplicationListService _leaveApplicationListService;
    private readonly ILeaveTypesService _leaveTypeService;
    private readonly ILeaveRelieversService _leaveRelieversService;
    private readonly ICacheService _cache;
    private readonly ILogger<LeaveService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly BCSettings _bcSettings;

    public LeaveService(ICacheService cacheService, 
        ILogger<LeaveService> logger, 
        IUnitOfWork unitOfWork,
        IOptions<BCSettings> bcSettings,
        INavisionService navisionService,
        ILeaveApplicationCardService leaveApplicationCardService,
        ILeaveApplicationListService leaveApplicationListService,
        ILeaveTypesService leaveTypeService,
        ILeaveRelieversService leaveRelieversService
        )
    {
        _cache = cacheService;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _bcSettings = bcSettings.Value;
        _navisionService = navisionService;
        _leaveApplicationCardService = leaveApplicationCardService;
        _leaveApplicationListService = leaveApplicationListService;
        _leaveTypeService = leaveTypeService;
        _leaveRelieversService = leaveRelieversService;

    }




    public async Task<AppResponse<LeaveApplicationResponse>> CreateLeaveApplicationAsync(CreateLeaveApplicationRequest request)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("CreateLeaveApplication", out var createLeaveApplicationEntitySet))
                return AppResponse<LeaveApplicationResponse>.Failure("Leave Application entity set not configured");

            var leaveApplicationCard = request.ToCreateLeaveApplicationCard();

            var createLeaveResponse = await _navisionService.CreateLeaveApplicationAsync(createLeaveApplicationEntitySet, leaveApplicationCard);

            if (!createLeaveResponse.Successful)
            {
                _logger.LogError("Failed to create leave application in BC: {Message}", createLeaveResponse.Message);

                return AppResponse<LeaveApplicationResponse>.Failure(createLeaveResponse.Message ?? "Failed to submit leave application");
            }

            _cache.Remove(CacheKeys.LeaveHistory(request.EmployeeNo));
            _cache.InvalidateLeaveSummary(request.EmployeeNo);
            _cache.InvalidateDashboard(request.EmployeeNo);
            _cache.InvalidateAllUserCaches(request.EmployeeNo);

            var result = new LeaveApplicationResponse
            {
                ApplicationNo = string.Empty,
                Status = "Submitted",
                Message = "Leave application submitted successfully"
            };

            return AppResponse<LeaveApplicationResponse>.Success("Leave application submitted successfully", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave application for employee {EmployeeNo}", request.EmployeeNo);
            throw;
        }
    }

    public async Task<AppResponse<LeaveApplicationResponse>> UpdateLeaveApplicationAsync(CreateLeaveApplicationRequest request)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("UpdateLeaveApplication", out var updateLeaveApplicationEntitySet))
                return AppResponse<LeaveApplicationResponse>.Failure("Leave Application entity set not configured");

            var leaveApplicationCard = request.ToUpdateLeaveApplicationCard();

            var createLeaveResponse = await _navisionService.UpdateLeaveApplicationAsync(updateLeaveApplicationEntitySet, leaveApplicationCard);

            if (!createLeaveResponse.Successful)
            {
                _logger.LogError("Failed to create leave application in BC: {Message}", createLeaveResponse.Message);

                return AppResponse<LeaveApplicationResponse>.Failure(createLeaveResponse.Message ?? "Failed to submit leave application");
            }

            _cache.Remove(CacheKeys.LeaveHistory(request.EmployeeNo));
            _cache.InvalidateLeaveSummary(request.EmployeeNo);
            _cache.InvalidateDashboard(request.EmployeeNo);
            _cache.InvalidateAllUserCaches(request.EmployeeNo);

            var result = new LeaveApplicationResponse
            {
                ApplicationNo = string.Empty,
                Status = "Submitted",
                Message = "Leave application updated successfully"
            };

            return AppResponse<LeaveApplicationResponse>.Success("Leave application submitted successfully", result);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error updating leave application for employee {EmployeeNo}", request.EmployeeNo);
            throw;
        }
    }

    public async Task<AppResponse<PagedResult<LeaveHistoryResponse>>> GetLeaveHistoryAsync(string employeeNo)
    {
        try
        {
            if (!_bcSettings.EntitySets.TryGetValue("LeaveApplicationLists", out var entitySet))
                return AppResponse<PagedResult<LeaveHistoryResponse>>.Failure("Leave Application Lists entity set not configured");

            LeaveApplicationCardFilter filter = new() { Employee_No = employeeNo };
            var odataQuery = filter.BuildODataFilter();
            var requestUri = string.IsNullOrWhiteSpace(odataQuery) ? entitySet : $"{entitySet}?{odataQuery}";

            var response = await _navisionService.GetMultipleAsync<Domain.Entities.LeaveApplicationCard>(requestUri);
            if (!response.Successful)
            {
                _logger.LogError("Failed to fetch leave history for employee {EmployeeNo}: {Message}", employeeNo, response.Message);
                return AppResponse<PagedResult<LeaveHistoryResponse>>.Failure(response.Message ?? "Failed to fetch leave history");
            }

            var (items, _) = response.Data;

            var pagedResult = await HandleNavisionPagedResponse(response);
            if (!pagedResult.Successful)
            {
                _logger.LogError("Failed to handle paged response for leave history: {Message}", pagedResult.Message);
                return AppResponse<PagedResult<LeaveHistoryResponse>>.Failure(pagedResult.Message ?? "We encountered a problem fetching your leave history.");
            }

            // Map to response DTOs
            var leaveHistoryResponses = items.ToLeaveHistoryResponses();
            if (leaveHistoryResponses == null || !leaveHistoryResponses.Any())
            {
                return AppResponse<PagedResult<LeaveHistoryResponse>>.Success("No leave history found", new PagedResult<LeaveHistoryResponse>());
            }

            var result = new PagedResult<LeaveHistoryResponse>
            {
                Items = leaveHistoryResponses,
                TotalCount = leaveHistoryResponses.Count,
                Cursor = pagedResult.Data?.Cursor,
                NextCursor = pagedResult.Data?.NextCursor,
                PageSize = pagedResult.Data?.PageSize ?? 0,
                CurrentPage = pagedResult.Data?.CurrentPage ?? 0,
                IsFirstPage = pagedResult.Data?.IsFirstPage ?? false,
                IsLastPage = pagedResult.Data?.IsLastPage ?? false,
                TotalPages = pagedResult.Data?.TotalPages ?? 0

            };


            return AppResponse<PagedResult<LeaveHistoryResponse>>.Success("Leave history fetched successfully", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching leave history for employee {EmployeeNo}", employeeNo);
            throw;
        }
    }

    public async Task<AppResponse<AnnualLeaveSummaryResponse>> GetAnnualLeaveSummaryAsync(string employeeNo)
    {
        try
        {
            var cached = _cache.GetAnnualLeaveSummary(employeeNo);
            if (cached != null)
                return AppResponse<AnnualLeaveSummaryResponse>.Success(cached);

            var currentLeavePeriod = DateTime.Now.Year.ToString();

            var leaveApplicationListsTask = GetLeaveApplicationListsAsync(employeeNo);
            var leaveApplicationCardsTask = GetLeaveApplicationCardsAsync(employeeNo);

            await Task.WhenAll(leaveApplicationListsTask, leaveApplicationCardsTask);

            var leaveApplicationListEntities = leaveApplicationListsTask.Result;
            var leaveApplicationCardEntities = leaveApplicationCardsTask.Result;

            var leaveApplicationCards = leaveApplicationCardEntities.Items.ToLeaveApplicationCards();
            var leaveApplicationLists = leaveApplicationListEntities.Items.ToLeaveApplicationLists();

            var leaveSummary = LeaveSummaryCalculator.CalculateLeaveSummary(
                leaveApplicationCards,
                leaveApplicationLists,
                currentLeavePeriod);

            if (leaveSummary == null)
            {
                return AppResponse<AnnualLeaveSummaryResponse>.Success("No leave summary found", new AnnualLeaveSummaryResponse());
            }

            _cache.SetAnnualLeaveSummary(employeeNo, leaveSummary);

            return AppResponse<AnnualLeaveSummaryResponse>.Success("Leave summary fetched successfully", leaveSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving annual leave summary");
            throw;
        }
    }

    public async Task<AppResponse<LeaveSummaryResponse>> GetLeaveSummaryAsync(string employeeNo)
    {
        try
        {
            var cached = _cache.GetLeaveSummary(employeeNo);
            if (cached != null)
                return AppResponse<LeaveSummaryResponse>.Success(cached);

            var leaveApplicationCardsTask = GetLeaveApplicationCardsAsync(employeeNo);
            var leaveTypesTask = GetActiveLeaveTypesAsync();

            await Task.WhenAll(leaveApplicationCardsTask,leaveTypesTask);

            var leaveApplicationCardEntities = leaveApplicationCardsTask.Result;
            var leaveTypesEntities = leaveTypesTask.Result;

            var leaveApplicationCards = leaveApplicationCardEntities.Items.ToLeaveApplicationCards();

            var leaveTypeResponses = LeaveMappingExtensions.ToLeaveTypeResponses(leaveTypesEntities);

            var leaveSummary = BCEntityMappingExtensions.ToLeaveSummaryResponse(
                employeeNo,
                leaveTypesEntities,
                leaveApplicationCards);

            _cache.SetLeaveSummary(employeeNo, leaveSummary);

            return AppResponse<LeaveSummaryResponse>.Success("Leave summary retrieved successfully", leaveSummary);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving leave summary");
            throw;
        }
    }

    private async Task<PagedResult<LeaveApplicationCardResponse>> GetLeaveApplicationCardsAsync(string employeeNo)
    {
        try
        {
            LeaveApplicationCardFilter filter = new() { Employee_No = employeeNo };

            var response = await _leaveApplicationCardService.SearchLeaveApplicationCardsAsync(filter);
            if (!response.Successful)
            {
                _logger.LogError("Failed to fetch leave application cards for employee {EmployeeNo}: {Message}", employeeNo, response.Message);
                return new();
            }

            if (response.Data == null || response.Data.Items == null || !response.Data.Items.Any())
            {
                return new();
            }

            return response.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching leave application cards for employee {EmployeeNo}", employeeNo);
            throw;
        }
    }

    private async Task<PagedResult<LeaveApplicationListResponse>> GetLeaveApplicationListsAsync(string employeeNo)
    {
        try
        {
            LeaveApplicationListFilter filter = new() { EmployeeNo = employeeNo };

            var response = await _leaveApplicationListService.SearchLeaveApplicationListsAsync(filter);
            if (!response.Successful)
            {
                _logger.LogError("Failed to fetch leave application lists for employee {EmployeeNo}: {Message}", employeeNo, response.Message);
                return new ();
            }

            if (response.Data == null || response.Data.Items == null || !response.Data.Items.Any())
            {
                return new ();
            }

            return response.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching leave application lists for employee {EmployeeNo}", employeeNo);
            throw;
        }

    }

    private async Task<List<LeaveTypes>> GetActiveLeaveTypesAsync()
    {
        try
        {
            var cachedLeaveTypes = _cache.GetLeaveTypes();
            if (cachedLeaveTypes != null)
            {
                return cachedLeaveTypes.Select(lt => new LeaveTypes
                {
                    Code = lt.Code,
                    Description = lt.Description,
                    Max_Carry_Forward_Days = lt.MaxDays,
                    Days = lt.Days,
                    Gender = lt.Gender,
                    Annual_Leave = lt.AnnualLeave

                }).ToList();
            }

            var response = await _leaveTypeService.GetLeaveTypesAsync();
            if (!response.Successful)
            {
                _logger.LogError("Failed to fetch leave types: {Message}", response.Message);
                return [];
            }

            if (response.Data == null || response.Data.Items == null || !response.Data.Items.Any())
            {
                return [];
            }

            var leaveTypeResponses = response.Data.Items;

            _cache.SetLeaveTypes(leaveTypeResponses);

            var leaveTypes = LeaveMappingExtensions.ToLeaveTypes(leaveTypeResponses);

            return leaveTypes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching active leave types");

            throw;
        }
    }

    private static Task<AppResponse<PagedResult<T>>> HandleNavisionPagedResponse<T>(AppResponse<(List<T> Items, string RawJson)> response)
    {
        if (!response.Successful)
        {
            return Task.FromResult(
                AppResponse<PagedResult<T>>.Failure(response.Message ?? "Failed to fetch records."));
        }

        var pagedResult = JsonSerializer.Deserialize<PagedResult<T>>(response.Data.RawJson);

        return Task.FromResult(
            pagedResult == null
                ? AppResponse<PagedResult<T>>.Failure("Failed to deserialize response.")
                : AppResponse<PagedResult<T>>.Success("Success", pagedResult));
    }



}
