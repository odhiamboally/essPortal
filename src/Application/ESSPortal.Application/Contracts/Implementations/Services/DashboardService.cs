using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;

using ESSPortal.Application.Extensions;
using ESSPortal.Application.Mappings;
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.NavEntities;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Dashboard;
using ESSPortal.Shared.Dtos.Leave;
using ESSPortal.Shared.Dtos.ModelFilters;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using LeaveApplicationCard = ESSPortal.Domain.Entities.LeaveApplicationCard;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class DashboardService(ICacheService cacheService,
    ILogger<DashboardService> logger,
    IUnitOfWork unitOfWork,
    IOptions<BCSettings> bcSettings,

    ILeaveApplicationCardService leaveApplicationCardService,
    ILeaveApplicationListService leaveApplicationListService,
    ILeaveTypesService leaveTypeService,
    IApprovedLeaveService approvedLeaveService,
    IEmployeeService employeeService

        ) : IDashboardService
{
    private readonly ILeaveApplicationCardService _leaveApplicationCardService = leaveApplicationCardService;
    private readonly ILeaveApplicationListService _leaveApplicationListService = leaveApplicationListService;
    private readonly ILeaveTypesService _leaveTypeService = leaveTypeService;
    private readonly IApprovedLeaveService _approvedLeaveService = approvedLeaveService;
    private readonly IEmployeeService _employeeService = employeeService;

    private readonly ICacheService _cache = cacheService;
    private readonly ILogger<DashboardService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly BCSettings _bcSettings = bcSettings.Value;

    public async Task<AppResponse<DashboardResponse>> GetDashboardDataAsync(string employeeNo)
    {
        try
        {
            // Try cache first
            var cachedData = _cache.GetDashboard(employeeNo);
            if (cachedData != null)
            {
                return AppResponse<DashboardResponse>.Success("Dashboard data retrieved from cache", cachedData);
            }

            // Fetch fresh data
            var response = await FetchFreshDashboardDataAsync(employeeNo);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data for employee {EmployeeNo}", employeeNo);
            throw;
        }
    }

    private async Task<AppResponse<DashboardResponse>> FetchFreshDashboardDataAsync(string employeeNo)
    {
        var approvedLeavedTask = GetLeaveApprovedLeavesAsync(employeeNo);
        var leaveApplicationCardsTask = GetLeaveApplicationCardsAsync(employeeNo);
        var leaveTypesTask = GetActiveLeaveTypesAsync();
        var leaveRelieversTask = GetLeaveRelieversAsync(employeeNo);

        await Task.WhenAll(
            approvedLeavedTask,
            leaveApplicationCardsTask,
            leaveTypesTask,
            leaveRelieversTask
            );

        var approvedLeavesEntities = approvedLeavedTask.Result;
        var leaveApplicationCardEntities = leaveApplicationCardsTask.Result;
        var leaveTypesEntities = leaveTypesTask.Result;
        var leaveRelieversEntities = leaveRelieversTask.Result;

        var leaveTypes = BCEntityMappingExtensions.ToLeaveTypeResponses(leaveTypesEntities);
        var leaveApplicationCards = leaveApplicationCardEntities.Items.ToLeaveApplicationCards();

        var annualLeaveSummary = BCEntityMappingExtensions.ToAnnualLeaveSummaryResponse(
                employeeNo,
                approvedLeavesEntities,
                leaveApplicationCards,
                leaveTypesEntities
        );

        var leaveSummary = BCEntityMappingExtensions.ToLeaveSummaryResponse(
                employeeNo,
                leaveTypesEntities,
                leaveApplicationCards);

        var leaveHistory = leaveApplicationCards.ToLeaveHistoryResponses(leaveTypes);
        var approvedLeaveResponses = approvedLeavesEntities?.ToApprovedLeaveResponses() ?? [];
        var leaveApplicationCardResponse = leaveApplicationCards.ToLeaveApplicationCardResponse();

        var leaveRelievers = leaveRelieversEntities;

        var employeeName = leaveApplicationCards?.FirstOrDefault()?.Employee_Name ?? "Unknown Employee";

        var response = new DashboardResponse(
            employeeNo,
            employeeName,
            annualLeaveSummary,
            leaveSummary,
            leaveHistory,
            approvedLeaveResponses,
            leaveApplicationCardResponse,
            [],
            leaveTypes,
            leaveRelievers
        );

        _cache.SetDashboard(employeeNo, response);

        return AppResponse<DashboardResponse>.Success("Dashboard data retrieved successfully", response);
    }

    private async Task<PagedResult<LeaveApplicationCardResponse>> GetLeaveApplicationCardsAsync(string employeeNo)
    {
        LeaveApplicationCardFilter filter = new() { Employee_No = employeeNo };

        var response = await _leaveApplicationCardService.SearchLeaveApplicationCardsAsync(filter);
        if (!response.Successful)
        {
            return new();
        }

        if (response.Data == null || response.Data.Items == null || !response.Data.Items.Any())
        {
            return new();
        }

        return response.Data;
    }

    private async Task<List<ApprovedLeaves>> GetLeaveApprovedLeavesAsync(string employeeNo)
    {
        ApprovedLeaveFilter filter = new() { Employee_No = employeeNo };

        var response = await _approvedLeaveService.SearchLeaveApplicationCardsAsync(filter);
        if (!response.Successful)
        {
            _logger.LogError("Failed to fetch leave application lists for employee {EmployeeNo}: {Message}", employeeNo, response.Message);
            return [];
        }
        if (response.Data == null || response.Data.Items == null || !response.Data.Items.Any())
        {
            return [];
        }
        return response.Data.Items;

    }

    private async Task<List<LeaveTypes>> GetActiveLeaveTypesAsync()
    {
        var cachedLeaveTypes = _cache.GetLeaveTypes();
        if (cachedLeaveTypes != null)
        {
            return [.. cachedLeaveTypes.Select(lt => new LeaveTypes
            {
                Code = lt.Code,
                Description = lt.Description,
                Max_Carry_Forward_Days = lt.MaxDays,
                Days = lt.Days,
                Gender = lt.Gender,
                Annual_Leave = lt.AnnualLeave

            })];
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

        var activeLeaveaTypes = leaveTypeResponses.ToLeaveTypes();

        return activeLeaveaTypes;
    }

    private async Task<List<LeaveRelieverResponse>> GetLeaveRelieversAsync(string employeeNo)
    {
        try
        {
            EmployeeCardFilter filter = new()
            {
                No = employeeNo,
            };

            var searchResponse = await _employeeService.SearchEmployeeCardsAsync(filter);
            if (!searchResponse.Successful || searchResponse.Data == null || !searchResponse.Data.Items.Any())
            {
                _logger.LogWarning("Failed to retrieve employee card for employee {EmployeeNo}: {Message}", employeeNo, searchResponse.Message);

                return [];
            }

            // Get the responsibility center from the employee card
            var employeeCardResponse = searchResponse.Data.Items.FirstOrDefault();
            if (employeeCardResponse == null || string.IsNullOrWhiteSpace(employeeCardResponse.ResponsibilityCenter))
            {
                return [];
            }

            EmployeeCardFilter leaveRelieverFilter = new()
            {
                Responsibility_Center = employeeCardResponse.ResponsibilityCenter
            };

            var potentialRelieversResponse = await _employeeService.SearchEmployeeCardsAsync(leaveRelieverFilter);

            if (!potentialRelieversResponse.Successful || potentialRelieversResponse.Data == null)
            {
                _logger.LogWarning("Failed to retrieve potential relievers for responsibility center {ResponsibilityCenter}: {Message}", employeeCardResponse.ResponsibilityCenter, potentialRelieversResponse.Message);
                    
                return [];
            }

            var potentialRelievers = 
                potentialRelieversResponse.Data.Items.Where(r => r.No != employeeNo).ToList() ?? [];

            if (!potentialRelievers.Any())
            {
                return [];
            }

            // Map employees to reliever responses
            var relieverResponses = potentialRelievers.Select(emp => new LeaveRelieverResponse
            {
                EmployeeNo = emp.No ?? string.Empty,
                EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                EmailAddress = emp.Email ?? string.Empty,
                Department = emp.ResponsibilityCenter ?? string.Empty,
                JobTitle = emp.JobPositionTitle ?? string.Empty

            }).ToList();

            return relieverResponses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving potential relievers for employee {EmployeeNo}", employeeNo);
            throw;
        }
    }

}
