

using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveService
{
    Task<AppResponse<LeaveApplicationResponse>> CreateLeaveApplicationAsync(CreateLeaveApplicationRequest request);
    Task<AppResponse<LeaveApplicationResponse>> UpdateLeaveApplicationAsync(CreateLeaveApplicationRequest request);
    Task<AppResponse<AnnualLeaveSummaryResponse>> GetAnnualLeaveSummaryAsync(string employeeNo);
    Task<AppResponse<LeaveSummaryResponse>> GetLeaveSummaryAsync(string employeeNo);
    Task<AppResponse<PagedResult<LeaveHistoryResponse>>> GetLeaveHistoryAsync(string employeeNo);
}
