
using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Domain.NavEntities;
using ESSPortal.Domain.NavEntities.LeaveApplication;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveRelieversService
{
    Task<AppResponse<bool>> CreateAsync(CreateLeaveRelieverRequest request);
    Task<AppResponse<bool>> CreateMultipleAsync(List<CreateLeaveRelieverRequest> requests);
    Task<AppResponse<LeaveRelieverResponse>> GetLeaveRelieverAsync(string leaveCode, string staffNo);
    Task<AppResponse<PagedResult<LeaveRelieverResponse>>> GetLeaveRelieversAsync();
    Task<AppResponse<PagedResult<LeaveRelieverResponse>>> GetLeaveRelieversByApplicationNoAsync(string applicationNo);
    Task<AppResponse<PagedResult<LeaveRelieverResponse>>> SearchLeaveRelieversAsync(LeaveRelieverFilter filter);

    


}
