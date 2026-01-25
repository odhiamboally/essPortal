
using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Domain.NavEntities;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ILeaveApplicationCardService
{
    Task<AppResponse<bool>> CreateLeaveApplicationCardAsync(CreateLeaveApplicationCardRequest request);
    Task<AppResponse<PagedResult<LeaveApplicationCardResponse>>> GetLeaveApplicationCardsAsync();
    Task<AppResponse<LeaveApplicationCardResponse>> GetLeaveApplicationCardByNoAsync(string applicationNo);
    Task<AppResponse<PagedResult<LeaveApplicationCardResponse>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter);

   
}
