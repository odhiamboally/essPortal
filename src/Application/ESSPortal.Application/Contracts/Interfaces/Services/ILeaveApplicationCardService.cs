using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveApplicationCardService
{
    Task<ApiResponse<PagedResult<LeaveApplicationCard>>> GetLeaveApplicationCardsAsync();
    Task<ApiResponse<LeaveApplicationCard>> GetLeaveApplicationCardByNoAsync(string applicationNo);
    Task<ApiResponse<PagedResult<Domain.Entities.LeaveApplicationCard>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter);

   
}
