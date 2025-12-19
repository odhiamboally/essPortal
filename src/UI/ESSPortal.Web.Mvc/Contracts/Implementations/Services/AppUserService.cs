using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Auth;
using EssPortal.Web.Mvc.Dtos.Common;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Services;

internal sealed class AppUserService : IAppUserService
{
    private readonly IApiService _apiService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApiSettings _apiSettings;
    public AppUserService(
        IApiService apiService, 
        IHttpContextAccessor httpContextAccessor, 
        IOptions<ApiSettings> apiSettings)
    {
        _apiService = apiService;
        _httpContextAccessor = httpContextAccessor;
        _apiSettings = apiSettings.Value;
    }
    

    public async Task<AppResponse<CurrentUserResponse>> GetCurrentUser()
    {
        var username = _httpContextAccessor.HttpContext?.User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            return AppResponse<CurrentUserResponse>.Failure("User not authenticated");
        }

        if (_apiSettings.ApiEndpoints?.Auth?.GetCurrentUser == null)
        {
            return AppResponse<CurrentUserResponse>.Failure("API endpoint for getting the current user is not configured");
        }

        var endpoint = _apiSettings.ApiEndpoints?.Auth.GetCurrentUser;
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return AppResponse<CurrentUserResponse>.Failure("API endpoint for getting the current user is not configured");
        }

        var apiResponse = await _apiService.GetAsync<CurrentUserResponse>(endpoint);

        if (apiResponse.Data == null)
        {
            return AppResponse<CurrentUserResponse>.Failure("Failed to retrieve current user");
        }

        return AppResponse<CurrentUserResponse>.Success(apiResponse.Message!, apiResponse.Data);
    }

    public async Task<AppResponse<string>> GetUserIdFromEmployeeNumber(string employeeNumber)
    {
        var endpoint = _apiSettings.ApiEndpoints?.User?.GetUserIdFromEmployeeNumber;
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return AppResponse<string>.Failure("API endpoint for getting user ID from employee number is not configured");
        }

        var response = await _apiService.GetAsync<string?>(endpoint);

        if (response.Data == null)
        {
            return AppResponse<string>.Failure("Failed to retrieve user ID");
        }

        return AppResponse<string>.Success(response.Message!, response.Data);
    }


}
