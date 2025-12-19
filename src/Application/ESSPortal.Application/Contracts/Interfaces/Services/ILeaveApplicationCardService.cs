using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveApplicationCardService
{
    Task<ApiResponse<bool>> CreateLeaveApplicationCardAsync(CreateLeaveApplicationCardRequest request);
    Task<ApiResponse<PagedResult<LeaveApplicationCardResponse>>> GetLeaveApplicationCardsAsync();
    Task<ApiResponse<LeaveApplicationCardResponse>> GetLeaveApplicationCardByNoAsync(string applicationNo);
    Task<ApiResponse<PagedResult<LeaveApplicationCardResponse>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter);

   
}
