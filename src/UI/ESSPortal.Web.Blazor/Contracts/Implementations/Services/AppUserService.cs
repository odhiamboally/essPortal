using EssPortal.Web.Blazor.Dtos.Auth;
using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.Services;

internal sealed class AppUserService : IAppUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceManager _serviceManager;
    private readonly IAppStateService _stateService;
    private readonly ILogger<AppUserService> _logger;

    public AppUserService(
        IHttpContextAccessor httpContextAccessor, 
        IServiceManager serviceManager, 
        IAppStateService stateService, 
        ILogger<AppUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceManager = serviceManager;
        _stateService = stateService;
        _logger = logger;
    }
    

    public async Task<ApiResponse<CurrentUserResponse>> GetCurrentUser()
    {
        try
        {
            var username = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                return ApiResponse<CurrentUserResponse>.Failure("User not authenticated");
            }

            var currentUser = await _stateService.LoadCurrentUserAsync();

            if (currentUser == null)
            {
                _logger.LogWarning("No current user data found");
                return ApiResponse<CurrentUserResponse>.Failure("Failed to retrieve current user");
            }

            return ApiResponse<CurrentUserResponse>.Success("Current user retrieved successfully", currentUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            throw;
        }
        
    }

    


}
