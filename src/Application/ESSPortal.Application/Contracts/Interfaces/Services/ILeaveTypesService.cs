using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveTypesService
{
    // Read operations
    Task<ApiResponse<bool>> CreateLeaveTypeAsync(CreateLeaveTypeRequest request);
    Task<ApiResponse<PagedResult<LeaveTypeResponse>>> GetLeaveTypesAsync();
    Task<ApiResponse<LeaveTypeResponse>> GetLeaveTypeByCodeAsync(string code);
    Task<ApiResponse<PagedResult<LeaveTypeResponse>>> SearchLeaveTypesAsync(LeaveTypeFilter filter);

   
}
