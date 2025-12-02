using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveService
{
    Task<ApiResponse<LeaveApplicationResponse>> CreateLeaveApplicationAsync(CreateLeaveApplicationRequest request);
    Task<ApiResponse<LeaveApplicationResponse>> UpdateLeaveApplicationAsync(CreateLeaveApplicationRequest request);
    Task<ApiResponse<AnnualLeaveSummaryResponse>> GetAnnualLeaveSummaryAsync(string employeeNo);
    Task<ApiResponse<LeaveSummaryResponse>> GetLeaveSummaryAsync(string employeeNo);
    Task<ApiResponse<PagedResult<LeaveHistoryResponse>>> GetLeaveHistoryAsync(string employeeNo);

}
