using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Dtos.Common;
using EssPortal.Web.Blazor.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Models.Navision;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface ILeaveStatisticsFactboxService
{
    Task<ApiResponse<PagedResult<LeaveStatisticsFactboxResponse>>> GetLeaveStatisticsAsync();
    Task<ApiResponse<PagedResult<LeaveStatisticsFactboxResponse>>> SearchLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter);

   
}
