using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveStatisticsFactboxService
{
    Task<ApiResponse<PagedResult<LeaveStatisticsFactbox>>> GetLeaveStatisticsAsync();
    Task<ApiResponse<PagedResult<LeaveStatisticsFactbox>>> SearchLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter);

   
}
