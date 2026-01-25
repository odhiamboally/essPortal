
using ESSPortal.Domain.NavEntities;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.ModelFilters;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IApprovedLeaveService
{
    Task<AppResponse<PagedResult<ApprovedLeaves>>> SearchLeaveApplicationCardsAsync(ApprovedLeaveFilter filter);
}
