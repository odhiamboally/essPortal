using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Dtos.Common;
using EssPortal.Web.Blazor.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Models.Navision;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface ILeaveApplicationCardService
{
    Task<ApiResponse<bool>> CreateLeaveApplicationCardAsync(CreateLeaveApplicationCardRequest request);
    Task<ApiResponse<PagedResult<LeaveApplicationCardResponse>>> GetLeaveApplicationCardsAsync();
    Task<ApiResponse<LeaveApplicationCardResponse?>> GetLeaveApplicationCardByNoAsync(string applicationNo);
    Task<ApiResponse<PagedResult<LeaveApplicationCardResponse>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter);

}
