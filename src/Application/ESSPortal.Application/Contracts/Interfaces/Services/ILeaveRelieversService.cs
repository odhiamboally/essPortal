using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Domain.NavEntities;
using ESSPortal.Domain.NavEntities.LeaveApplication;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveRelieversService
{
    // Read operations
    Task<ApiResponse<PagedResult<LeaveRelievers>>> GetLeaveRelieversAsync();
    Task<ApiResponse<LeaveRelievers>> GetLeaveRelieverAsync(string leaveCode, string staffNo);
    Task<ApiResponse<List<LeaveRelievers>>> GetLeaveRelieversByApplicationNoAsync(string applicationNo);
    Task<ApiResponse<PagedResult<LeaveRelievers>>> SearchLeaveRelieversAsync(LeaveRelieversFilter filter);

    Task<ApiResponse<bool>> CreateAsync(LeaveReliever leaveRelieve);
    Task<ApiResponse<bool>> CreateAsync(LeaveRelievers leaveReliever);
    Task<ApiResponse<bool>> CreateMultipleAsync(List<LeaveRelievers> leaveRelievers);


}
