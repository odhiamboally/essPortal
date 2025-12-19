using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Web.Blazor.Dtos.Leave;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface ILeaveService
{
    Task<ApiResponse<LeaveApplicationResponse>> CreateLeaveApplicationAsync(CreateLeaveApplicationRequest request);
    Task<ApiResponse<LeaveApplicationResponse>> EditLeaveApplicationAsync(CreateLeaveApplicationRequest request);
    Task<ApiResponse<PagedResult<LeaveHistoryResponse>>> GetLeaveHistoryAsync(string employeeNo);
    Task<ApiResponse<LeaveSummaryResponse>> GetLeaveSummaryAsync(string employeeNo);
    Task<ApiResponse<AnnualLeaveSummaryResponse>> GetAnnualLeaveSummaryAsync(string employeeNo);
    Task<ApiResponse<LeaveApplicationFormResponse>> GetLeaveApplicationFormDataAsync(string employeeNo);

}
