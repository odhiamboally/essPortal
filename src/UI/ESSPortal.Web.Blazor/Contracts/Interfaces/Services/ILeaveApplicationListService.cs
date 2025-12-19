using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Dtos.Common;
using EssPortal.Web.Blazor.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Models.Navision;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface ILeaveApplicationListService
{
    Task<ApiResponse<bool>> CreateLeaveApplicationListAsync(CreateLeaveApplicationListRequest request);
    Task<ApiResponse<PagedResult<LeaveApplicationListResponse>>> GetLeaveApplicationListsAsync();
    Task<ApiResponse<LeaveApplicationListResponse?>> GetLeaveApplicationListByNoAsync(string applicationNo);
    Task<ApiResponse<PagedResult<LeaveApplicationListResponse>>> SearchLeaveApplicationListsAsync(LeaveApplicationListFilter filter);

    
}
