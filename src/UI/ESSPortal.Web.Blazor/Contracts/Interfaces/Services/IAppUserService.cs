using EssPortal.Web.Blazor.Dtos.Auth;
using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Common;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface IAppUserService
{
    Task<ApiResponse<CurrentUserResponse>> GetCurrentUser();
}
