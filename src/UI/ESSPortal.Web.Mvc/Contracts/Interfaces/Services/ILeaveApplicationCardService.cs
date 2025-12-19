using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveApplicationCardService
{
    // Read operations
    Task<AppResponse<List<LeaveApplicationCard>>> GetLeaveApplicationCardsAsync();
    Task<AppResponse<LeaveApplicationCard?>> GetLeaveApplicationCardByNoAsync(string applicationNo);
    Task<AppResponse<LeaveApplicationCard?>> GetLeaveApplicationCardByRecIdAsync(string recId);
    Task<AppResponse<List<LeaveApplicationCard>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter);

    // Create operations
    Task<AppResponse<LeaveApplicationCard>> CreateLeaveApplicationCardAsync(LeaveApplicationCard request);
    Task<AppResponse<List<LeaveApplicationCard>>> CreateMultipleLeaveApplicationCardsAsync(List<LeaveApplicationCard> requests);

    // Update operations
    Task<AppResponse<LeaveApplicationCard>> EditLeaveApplicationCardAsync(LeaveApplicationCard request);
    Task<AppResponse<List<LeaveApplicationCard>>> UpdateMultipleLeaveApplicationCardsAsync(List<LeaveApplicationCard> requests);

    // Delete operations
    Task<AppResponse<bool>> DeleteLeaveApplicationCardAsync(string key);

    // Utility operations
    Task<AppResponse<string?>> GetLeaveApplicationCardRecIdFromKeyAsync(string key);
    Task<AppResponse<bool>> IsLeaveApplicationCardUpdatedAsync(string key);
}
