using EssPortal.Web.Mvc.Dtos.Auth;
using EssPortal.Web.Mvc.Dtos.Common;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface IAppUserService
{
    Task<AppResponse<CurrentUserResponse>> GetCurrentUser();
    Task<AppResponse<string>> GetUserIdFromEmployeeNumber(string employeeNumber);
}
