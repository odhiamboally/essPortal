using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Domain.NavEntities;
using ESSPortal.Domain.NavEntities.LeaveApplication;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveRelieversService
{
    Task<ApiResponse<bool>> CreateAsync(CreateLeaveRelieverRequest request);
    Task<ApiResponse<bool>> CreateMultipleAsync(List<CreateLeaveRelieverRequest> requests);
    Task<ApiResponse<LeaveRelieverResponse>> GetLeaveRelieverAsync(string leaveCode, string staffNo);
    Task<ApiResponse<PagedResult<LeaveRelieverResponse>>> GetLeaveRelieversAsync();
    Task<ApiResponse<PagedResult<LeaveRelieverResponse>>> GetLeaveRelieversByApplicationNoAsync(string applicationNo);
    Task<ApiResponse<PagedResult<LeaveRelieverResponse>>> SearchLeaveRelieversAsync(LeaveRelieversFilter filter);

    


}
