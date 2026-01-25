using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveApplicationCardService
{
    // Read operations
    Task<AppResponse<List<LeaveApplicationCardResponse>>> GetLeaveApplicationCardsAsync();
    Task<AppResponse<LeaveApplicationCardResponse?>> GetLeaveApplicationCardByNoAsync(string applicationNo);
    Task<AppResponse<LeaveApplicationCardResponse?>> GetLeaveApplicationCardByRecIdAsync(string recId);
    Task<AppResponse<List<LeaveApplicationCardResponse>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter);

    // Create operations
    Task<AppResponse<LeaveApplicationCardResponse>> CreateLeaveApplicationCardAsync(CreateLeaveApplicationCardRequest request);
    Task<AppResponse<List<LeaveApplicationCardResponse>>> CreateMultipleLeaveApplicationCardsAsync(List<CreateLeaveApplicationCardRequest> requests);

    // Update operations
    Task<AppResponse<LeaveApplicationCardResponse>> EditLeaveApplicationCardAsync(CreateLeaveApplicationCardRequest request);
    Task<AppResponse<List<LeaveApplicationCardResponse>>> UpdateMultipleLeaveApplicationCardsAsync(List<CreateLeaveApplicationCardRequest> requests);

    // Delete operations
    Task<AppResponse<bool>> DeleteLeaveApplicationCardAsync(string key);

    // Utility operations
    Task<AppResponse<string?>> GetLeaveApplicationCardRecIdFromKeyAsync(string key);
    Task<AppResponse<bool>> IsLeaveApplicationCardUpdatedAsync(string key);
}
