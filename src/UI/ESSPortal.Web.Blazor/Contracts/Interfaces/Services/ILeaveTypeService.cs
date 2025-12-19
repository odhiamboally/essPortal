using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Dtos.Common;
using EssPortal.Web.Blazor.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Models.Navision;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface ILeaveTypeService
{
    Task<ApiResponse<bool>> CreateLeaveTypeAsync(CreateLeaveTypeRequest request);
    Task<ApiResponse<PagedResult<LeaveTypeResponse>>> GetLeaveTypesAsync();
    Task<ApiResponse<LeaveTypeResponse?>> GetLeaveTypeByCodeAsync(string code);
    Task<ApiResponse<PagedResult<LeaveTypeResponse>>> SearchLeaveTypesAsync(LeaveTypeFilter filter);


}
