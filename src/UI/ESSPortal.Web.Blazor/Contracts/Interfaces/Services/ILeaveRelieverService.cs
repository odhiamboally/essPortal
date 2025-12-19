using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Dtos.Common;
using EssPortal.Web.Blazor.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Models.Navision;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface ILeaveRelieverService
{
    Task<ApiResponse<bool>> CreateLeaveRelieverAsync(CreateLeaveRelieverRequest request);
    Task<ApiResponse<bool>> CreateMultipleLeaveRelieversAsync(List<CreateLeaveRelieverRequest> requests);
    Task<ApiResponse<LeaveRelieverResponse?>> GetLeaveRelieverAsync(string leaveCode, string staffNo);
    Task<ApiResponse<PagedResult<LeaveRelieverResponse>>> GetLeaveRelieversAsync();
    Task<ApiResponse<PagedResult<LeaveRelieverResponse>>> GetLeaveRelieversByApplicationNoAsync(string applicationNo);
    Task<ApiResponse<PagedResult<LeaveRelieverResponse>>> SearchLeaveRelieversAsync(LeaveRelieversFilter filter);

    

}
