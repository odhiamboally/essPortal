using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveApplicationListService
{
    // Read operations
    Task<AppResponse<List<LeaveApplicationListResponse>>> GetLeaveApplicationListsAsync();
    Task<AppResponse<LeaveApplicationListResponse?>> GetLeaveApplicationListByNoAsync(string applicationNo);
    Task<AppResponse<LeaveApplicationListResponse?>> GetLeaveApplicationListByRecIdAsync(string recId);
    Task<AppResponse<List<LeaveApplicationListResponse>>> SearchLeaveApplicationListsAsync(LeaveApplicationListFilter filter);

    // Create operations
    Task<AppResponse<LeaveApplicationListResponse>> CreateLeaveApplicationListAsync(CreateLeaveApplicationListRequest request);
    Task<AppResponse<List<LeaveApplicationListResponse>>> CreateMultipleLeaveApplicationListsAsync(List<CreateLeaveApplicationListRequest> requests);

    // Update operations
    Task<AppResponse<LeaveApplicationListResponse>> EditLeaveApplicationListAsync(CreateLeaveApplicationListRequest request);
    Task<AppResponse<List<LeaveApplicationListResponse>>> UpdateMultipleLeaveApplicationListsAsync(List<CreateLeaveApplicationListRequest> requests);

    // Delete operations
    Task<AppResponse<bool>> DeleteLeaveApplicationListAsync(string key);

    // Utility operations
    Task<AppResponse<string?>> GetLeaveApplicationListRecIdFromKeyAsync(string key);
    Task<AppResponse<bool>> IsLeaveApplicationListUpdatedAsync(string key);
}
