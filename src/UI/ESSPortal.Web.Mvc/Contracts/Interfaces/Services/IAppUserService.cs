using EssPortal.Shared.Dtos.Auth;

using ESSPortal.Shared.Dtos.Common;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface IAppUserService
{
    Task<AppResponse<CurrentUserResponse>> GetCurrentUser();
    Task<AppResponse<string>> GetUserIdFromEmployeeNumber(string employeeNumber);
}
