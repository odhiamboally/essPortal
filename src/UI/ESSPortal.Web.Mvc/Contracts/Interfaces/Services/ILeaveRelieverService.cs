using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveRelieverService
{
    // Read operations
    Task<AppResponse<List<LeaveRelieverResponse>>> GetLeaveRelieversAsync(LeaveRelieverFilter filter);
    Task<AppResponse<LeaveRelieverResponse?>> GetLeaveRelieverAsync(string leaveCode, string staffNo);
    Task<AppResponse<LeaveRelieverResponse?>> GetLeaveRelieverByRecIdAsync(string recId);
    Task<AppResponse<List<LeaveRelieverResponse>>> GetLeaveRelieversByApplicationNoAsync(string applicationNo);
    Task<AppResponse<List<LeaveRelieverResponse>>> SearchLeaveRelieversAsync(LeaveRelieverFilter filter);

    // Create operations
    Task<AppResponse<LeaveRelieverResponse>> CreateLeaveRelieverAsync(CreateLeaveRelieverRequest request);
    Task<AppResponse<List<LeaveRelieverResponse>>> CreateMultipleLeaveRelieversAsync(List<CreateLeaveRelieverRequest> requests);

    // Update operations
    Task<AppResponse<LeaveRelieverResponse>> UpdateLeaveRelieverAsync(CreateLeaveRelieverRequest request);
    Task<AppResponse<List<LeaveRelieverResponse>>> UpdateMultipleLeaveRelieversAsync(List<CreateLeaveRelieverRequest> requests);

    // Delete operations
    Task<AppResponse<bool>> DeleteLeaveRelieverAsync(string key);

    // Utility operations
    Task<AppResponse<string?>> GetLeaveRelieverRecIdFromKeyAsync(string key);
    Task<AppResponse<bool>> IsLeaveRelieverUpdatedAsync(string key);

}
