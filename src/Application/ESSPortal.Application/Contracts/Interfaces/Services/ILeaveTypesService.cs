
using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Domain.NavEntities;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveTypesService
{
    // Read operations
    Task<AppResponse<bool>> CreateLeaveTypeAsync(CreateLeaveTypeRequest request);
    Task<AppResponse<PagedResult<LeaveTypeResponse>>> GetLeaveTypesAsync();
    Task<AppResponse<LeaveTypeResponse>> GetLeaveTypeByCodeAsync(string code);
    Task<AppResponse<PagedResult<LeaveTypeResponse>>> SearchLeaveTypesAsync(LeaveTypeFilter filter);

   
}
