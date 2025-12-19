using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveApplicationListService
{
    // Read operations
    Task<AppResponse<List<LeaveApplicationList>>> GetLeaveApplicationListsAsync();
    Task<AppResponse<LeaveApplicationList?>> GetLeaveApplicationListByNoAsync(string applicationNo);
    Task<AppResponse<LeaveApplicationList?>> GetLeaveApplicationListByRecIdAsync(string recId);
    Task<AppResponse<List<LeaveApplicationList>>> SearchLeaveApplicationListsAsync(LeaveApplicationListFilter filter);

    // Create operations
    Task<AppResponse<LeaveApplicationList>> CreateLeaveApplicationListAsync(LeaveApplicationList request);
    Task<AppResponse<List<LeaveApplicationList>>> CreateMultipleLeaveApplicationListsAsync(List<LeaveApplicationList> requests);

    // Update operations
    Task<AppResponse<LeaveApplicationList>> EditLeaveApplicationListAsync(LeaveApplicationList request);
    Task<AppResponse<List<LeaveApplicationList>>> UpdateMultipleLeaveApplicationListsAsync(List<LeaveApplicationList> requests);

    // Delete operations
    Task<AppResponse<bool>> DeleteLeaveApplicationListAsync(string key);

    // Utility operations
    Task<AppResponse<string?>> GetLeaveApplicationListRecIdFromKeyAsync(string key);
    Task<AppResponse<bool>> IsLeaveApplicationListUpdatedAsync(string key);
}
