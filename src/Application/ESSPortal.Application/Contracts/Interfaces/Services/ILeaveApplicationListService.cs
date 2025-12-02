using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveApplicationListService
{
    // Read operations
    Task<ApiResponse<PagedResult<LeaveApplicationList>>> GetLeaveApplicationListsAsync();
    Task<ApiResponse<LeaveApplicationList>> GetLeaveApplicationListByNoAsync(string applicationNo);
    Task<ApiResponse<PagedResult<LeaveApplicationList>>> SearchLeaveApplicationListsAsync(LeaveApplicationListFilter filter);

   
}
