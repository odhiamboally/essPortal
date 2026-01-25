using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveTypeService
{
    // Read operations
    Task<AppResponse<List<LeaveTypeResponse>>> GetLeaveTypesAsync();
    Task<AppResponse<LeaveTypeResponse?>> GetLeaveTypeByCodeAsync(string code);
    Task<AppResponse<LeaveTypeResponse?>> GetLeaveTypeByRecIdAsync(string recId);
    Task<AppResponse<List<LeaveTypeResponse>>> SearchLeaveTypesAsync(LeaveTypeFilter filter);

    // Create operations
    Task<AppResponse<LeaveTypeResponse>> CreateLeaveTypeAsync(CreateLeaveTypeRequest request);
    Task<AppResponse<List<LeaveTypeResponse>>> CreateMultipleLeaveTypesAsync(List<CreateLeaveTypeRequest> requests);

    // Update operations
    Task<AppResponse<LeaveTypeResponse>> UpdateLeaveTypeAsync(CreateLeaveTypeRequest request);
    Task<AppResponse<List<LeaveTypeResponse>>> UpdateMultipleLeaveTypesAsync(List<CreateLeaveTypeRequest> requests);

    // Delete operations
    Task<AppResponse<bool>> DeleteLeaveTypeAsync(string key);

    // Utility operations
    Task<AppResponse<string?>> GetLeaveTypeRecIdFromKeyAsync(string key);
    Task<AppResponse<bool>> IsLeaveTypeUpdatedAsync(string key);

}
