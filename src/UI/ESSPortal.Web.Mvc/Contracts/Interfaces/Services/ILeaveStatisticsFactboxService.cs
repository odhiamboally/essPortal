using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;
using ESSPortal.Shared.Dtos.Common;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveStatisticsFactboxService
{
    Task<AppResponse<List<LeaveStatisticsFactboxResponse>>> SearchLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter);

}
