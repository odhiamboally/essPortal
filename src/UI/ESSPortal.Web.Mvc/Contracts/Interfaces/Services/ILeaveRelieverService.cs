using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveRelieverService
{
    // Read operations
    Task<AppResponse<List<LeaveReliever>>> GetLeaveRelieversAsync(LeaveRelieverFilter filter);
    Task<AppResponse<LeaveReliever?>> GetLeaveRelieverAsync(string leaveCode, string staffNo);
    Task<AppResponse<LeaveReliever?>> GetLeaveRelieverByRecIdAsync(string recId);
    Task<AppResponse<List<LeaveReliever>>> GetLeaveRelieversByApplicationNoAsync(string applicationNo);
    Task<AppResponse<List<LeaveReliever>>> SearchLeaveRelieversAsync(LeaveRelieverFilter filter);

    // Create operations
    Task<AppResponse<LeaveReliever>> CreateLeaveRelieverAsync(LeaveReliever request);
    Task<AppResponse<List<LeaveReliever>>> CreateMultipleLeaveRelieversAsync(List<LeaveReliever> requests);

    // Update operations
    Task<AppResponse<LeaveReliever>> UpdateLeaveRelieverAsync(LeaveReliever request);
    Task<AppResponse<List<LeaveReliever>>> UpdateMultipleLeaveRelieversAsync(List<LeaveReliever> requests);

    // Delete operations
    Task<AppResponse<bool>> DeleteLeaveRelieverAsync(string key);

    // Utility operations
    Task<AppResponse<string?>> GetLeaveRelieverRecIdFromKeyAsync(string key);
    Task<AppResponse<bool>> IsLeaveRelieverUpdatedAsync(string key);

}
