using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.ModelFilters;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IApprovedLeaveService
{
    Task<ApiResponse<PagedResult<ApprovedLeaves>>> SearchLeaveApplicationCardsAsync(ApprovedLeaveFilter filter);
}
