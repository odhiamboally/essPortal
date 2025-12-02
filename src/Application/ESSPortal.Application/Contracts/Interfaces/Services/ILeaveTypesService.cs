using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveTypesService
{
    // Read operations
    Task<ApiResponse<PagedResult<LeaveTypes>>> GetLeaveTypesAsync();
    Task<ApiResponse<LeaveTypes>> GetLeaveTypeByCodeAsync(string code);
    Task<ApiResponse<PagedResult<LeaveTypes>>> SearchLeaveTypesAsync(LeaveTypeFilter filter);

   
}
