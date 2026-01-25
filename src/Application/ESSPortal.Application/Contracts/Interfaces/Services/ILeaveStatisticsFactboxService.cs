
using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Domain.NavEntities;
using ESSPortal.Shared.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveStatisticsFactboxService
{
    Task<AppResponse<PagedResult<LeaveStatisticsFactboxResponse>>> SearchLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter);

   
}
