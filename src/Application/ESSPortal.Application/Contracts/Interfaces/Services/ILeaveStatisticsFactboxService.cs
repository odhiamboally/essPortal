using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveStatisticsFactboxService
{
    Task<ApiResponse<PagedResult<LeaveStatisticsFactboxResponse>>> GetLeaveStatisticsAsync();
    Task<ApiResponse<PagedResult<LeaveStatisticsFactboxResponse>>> SearchLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter);

   
}
