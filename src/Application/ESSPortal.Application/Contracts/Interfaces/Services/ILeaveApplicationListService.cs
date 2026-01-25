
using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Domain.NavEntities;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveApplicationListService
{
    Task<AppResponse<bool>> CreateLeaveApplicationListAsync(CreateLeaveApplicationListRequest request);
    Task<AppResponse<PagedResult<LeaveApplicationListResponse>>> GetLeaveApplicationListsAsync();
    Task<AppResponse<LeaveApplicationListResponse?>> GetLeaveApplicationListByNoAsync(string applicationNo);
    Task<AppResponse<PagedResult<LeaveApplicationListResponse>>> SearchLeaveApplicationListsAsync(LeaveApplicationListFilter filter);

   
}
