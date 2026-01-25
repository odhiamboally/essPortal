
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveService
{
    Task<AppResponse<LeaveApplicationResponse>> CreateLeaveApplicationAsync(CreateLeaveApplicationRequest request);
    Task<AppResponse<LeaveApplicationResponse>> EditLeaveApplicationAsync(CreateLeaveApplicationRequest request);
    Task<AppResponse<PagedResult<LeaveHistoryResponse>>> GetLeaveHistoryAsync(string employeeNo);
    Task<AppResponse<LeaveSummaryResponse>> GetLeaveSummaryAsync(string employeeNo);

}
