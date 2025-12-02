using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveTypeService
{
    // Read operations
    Task<AppResponse<List<LeaveTypes>>> GetLeaveTypesAsync();
    Task<AppResponse<LeaveTypes?>> GetLeaveTypeByCodeAsync(string code);
    Task<AppResponse<LeaveTypes?>> GetLeaveTypeByRecIdAsync(string recId);
    Task<AppResponse<List<LeaveTypes>>> SearchLeaveTypesAsync(LeaveTypeFilter filter);

    // Create operations
    Task<AppResponse<LeaveTypes>> CreateLeaveTypeAsync(LeaveTypes request);
    Task<AppResponse<List<LeaveTypes>>> CreateMultipleLeaveTypesAsync(List<LeaveTypes> requests);

    // Update operations
    Task<AppResponse<LeaveTypes>> UpdateLeaveTypeAsync(LeaveTypes request);
    Task<AppResponse<List<LeaveTypes>>> UpdateMultipleLeaveTypesAsync(List<LeaveTypes> requests);

    // Delete operations
    Task<AppResponse<bool>> DeleteLeaveTypeAsync(string key);

    // Utility operations
    Task<AppResponse<string?>> GetLeaveTypeRecIdFromKeyAsync(string key);
    Task<AppResponse<bool>> IsLeaveTypeUpdatedAsync(string key);

}
