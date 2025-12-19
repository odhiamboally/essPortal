using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveApplicationListService
{
    Task<ApiResponse<bool>> CreateLeaveApplicationListAsync(CreateLeaveApplicationListRequest request);
    Task<ApiResponse<PagedResult<LeaveApplicationListResponse>>> GetLeaveApplicationListsAsync();
    Task<ApiResponse<LeaveApplicationListResponse?>> GetLeaveApplicationListByNoAsync(string applicationNo);
    Task<ApiResponse<PagedResult<LeaveApplicationListResponse>>> SearchLeaveApplicationListsAsync(LeaveApplicationListFilter filter);

   
}
