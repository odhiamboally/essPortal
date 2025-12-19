using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Dashboard;
using ESSPortal.Application.Utilities;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Dtos.Leave;

using MessagePack.Formatters;

using Microsoft.Extensions.Caching.Memory;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.Common;

internal sealed class AppStateService : IAppStateService
{
    private readonly IServiceManager _serviceManager;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AppStateService> _logger;

    public event Action? OnStateChanged;

    public AppStateService(
        IServiceManager serviceManager,
        IMemoryCache memoryCache,
        ILogger<AppStateService> logger)
    {
        _serviceManager = serviceManager;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Load dashboard data - this is the main data source
    /// Called once on app load, cached for session
    /// </summary>
    public async Task<DashboardResponse> LoadDashboardDataAsync(string employeeNo, bool forceRefresh = false)
    {
        try
        {
            var cacheKey = CacheKeys.Dashboard(employeeNo);
            if (!forceRefresh && _memoryCache.TryGetValue(cacheKey, out DashboardResponse? cached))
            {
                _logger.LogDebug("Using cached dashboard data for {EmployeeNo}", employeeNo);
                return cached ?? new(string.Empty, string.Empty, new(), new(), [], [], [], [], [], []);
            }

            _logger.LogInformation("Loading dashboard data for {EmployeeNo}", employeeNo);

            var response = await _serviceManager.DashboardService.GetDashboardDataAsync(employeeNo);

            if (response.Successful && response.Data != null)
            {
                // Cache with sliding expiration
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(CacheExpiration.Dashboard)
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .RegisterPostEvictionCallback((key, value, reason, state) =>
                    {
                        _logger.LogDebug("Dashboard cache evicted for {EmployeeNo}: {Reason}", employeeNo, reason);
                            
                    });

                _memoryCache.Set(cacheKey, response.Data, cacheOptions);

                _logger.LogInformation("Dashboard data cached successfully for {EmployeeNo}", employeeNo);

                NotifyStateChanged();
            }
            else
            {
                _logger.LogWarning("Failed to load dashboard data: {Message}", response.Message);
            }

            return response.Data ?? new(string.Empty, string.Empty, new(), new(), [], [], [], [], [], []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data for {EmployeeNo}", employeeNo);
            throw;
        }
    }

    /// <summary>
    /// Get leave form data - builds from cached dashboard data
    /// No backend call if dashboard is already loaded!
    /// </summary>
    public async Task<LeaveApplicationFormResponse> GetLeaveApplicationFormDataAsync(string employeeNo, bool forceRefresh = false)
    {
        var cacheKey = CacheKeys.LeaveFormData(employeeNo);

        if (!forceRefresh && _memoryCache.TryGetValue(cacheKey, out LeaveApplicationFormResponse? cached))
        {
            _logger.LogDebug("Using cached leave form data for {EmployeeNo}", employeeNo);
            return cached ?? new();
        }

        // Ensure dashboard data is loaded
        var dashboard = await LoadDashboardDataAsync(employeeNo, forceRefresh);

        var currentUser = await LoadCurrentUserAsync(forceRefresh);

        if (dashboard == null)
        {
            _logger.LogWarning("No dashboard data available for leave form");
            throw new InvalidOperationException("Dashboard data must be loaded first");
        }

        // Build form data from cached dashboard - NO backend call!
        var formData = new LeaveApplicationFormResponse
        {
            Employee = new LeaveApplicationEmployeeResponse
            {
                EmployeeNo = employeeNo,
                EmployeeName = $"{currentUser?.FirstName} {currentUser?.LastName}".Trim(),
                EmailAddress = currentUser?.Email ?? string.Empty,
                MobileNo = currentUser?.PhoneNumber ?? string.Empty,
                ResponsibilityCenter = dashboard.LeaveApplicationCards.FirstOrDefault()?.ResponsibilityCenter ?? string.Empty,
                BranchCode = "NAIROBI" // TODO: Get from user profile
            },
            AnnualLeaveSummary = dashboard.AnnualLeaveSummary ?? new(),
            LeaveSummary = dashboard.LeaveSummary ?? new(),
            LeaveTypes = dashboard.LeaveTypes ?? new(),
            AvailableRelievers = dashboard.LeaveRelievers ?? new()
        };

        // Cache the form data
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheExpiration.Dashboard)
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));

        _memoryCache.Set(cacheKey, formData, cacheOptions);

        _logger.LogInformation("Leave form data built from cache for {EmployeeNo}", employeeNo);

        return formData;
    }

    /// <summary>
    /// Load current user profile
    /// </summary>
    public async Task<CurrentUserResponse> LoadCurrentUserAsync(bool forceRefresh = false)
    {
        try
        {
            var cacheKey = CacheKeys.CurrentUser();

            if (!forceRefresh && _memoryCache.TryGetValue(cacheKey, out CurrentUserResponse? cached))
            {
                _logger.LogDebug("Using cached current user data");
                return cached ?? new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false, false, string.Empty, false, default, []);
            }

            _logger.LogInformation("Loading current user data");

            var apiResponse = await _serviceManager.AuthService.GetCurrentUserAsync();

            if (apiResponse.Successful && apiResponse.Data != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(CacheExpiration.CurrentUser)
                    .SetAbsoluteExpiration(TimeSpan.FromHours(2));

                _memoryCache.Set(cacheKey, apiResponse.Data, cacheOptions);

                _logger.LogInformation("Current user data cached successfully");

                NotifyStateChanged();
            }

            return apiResponse.Data ?? new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false, false, string.Empty, false, default, []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading current user data");
            throw;
        }
    }

    /// <summary>
    /// Clear all cached data
    /// </summary>
    public void ClearCache(string? employeeNo = null)
    {
        _logger.LogInformation("Clearing application cache for {EmployeeNo}", employeeNo ?? "session");

        if (!string.IsNullOrEmpty(employeeNo))
        {
            foreach (var key in CacheKeys.UserPattern(employeeNo))
            {
                _memoryCache.Remove(key);
            }
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Clear only leave form cache (e.g., after submission)
    /// </summary>
    public void ClearLeaveFormCache(string? employeeNo = null)
    {
        _logger.LogInformation("Clearing leave form cache for {EmployeeNo}", employeeNo ?? "session");

        if (!string.IsNullOrEmpty(employeeNo))
        {
            // ✅ Use CacheKeys utilities
            _memoryCache.Remove(CacheKeys.LeaveFormData(employeeNo));
            _memoryCache.Remove(CacheKeys.LeaveHistory(employeeNo));
            _memoryCache.Remove(CacheKeys.LeaveSummary(employeeNo));
            _memoryCache.Remove(CacheKeys.AnnualLeaveSummary(employeeNo));
        }

        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
