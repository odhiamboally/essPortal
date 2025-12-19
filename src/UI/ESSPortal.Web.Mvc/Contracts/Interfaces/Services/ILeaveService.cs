using EssPortal.Web.Mvc.Dtos.Common;
using ESSPortal.Web.Mvc.Dtos.Leave;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveService
{
    Task<AppResponse<LeaveApplicationResponse>> CreateLeaveApplicationAsync(CreateLeaveApplicationRequest request);
    Task<AppResponse<LeaveApplicationResponse>> EditLeaveApplicationAsync(CreateLeaveApplicationRequest request);
    Task<AppResponse<PagedResult<LeaveHistoryResponse>>> GetLeaveHistoryAsync(string employeeNo);
    Task<AppResponse<LeaveSummaryResponse>> GetLeaveSummaryAsync(string employeeNo);

}
