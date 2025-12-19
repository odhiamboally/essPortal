using Azure.Core;

using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Dtos.Common;
using EssPortal.Web.Blazor.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Models.Navision;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Employee;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using ESSPortal.Web.Blazor.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.Services;

internal sealed class LeaveApplicationCardService : ILeaveApplicationCardService
{
    private readonly IServiceManager _serviceManager;
    public LeaveApplicationCardService(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    #region LeaveApplicationCard

    public async Task<ApiResponse<bool>> CreateLeaveApplicationCardAsync(CreateLeaveApplicationCardRequest request)
    {
        var apiResponse = await _serviceManager.LeaveApplicationCardService.CreateLeaveApplicationCardAsync(request);
        return apiResponse.Successful
            ? ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data)
            : ApiResponse<bool>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<PagedResult<LeaveApplicationCardResponse>>> GetLeaveApplicationCardsAsync()
    {
        var apiResponse = await _serviceManager.LeaveApplicationCardService.GetLeaveApplicationCardsAsync();
        return apiResponse.Successful
            ? ApiResponse<PagedResult<LeaveApplicationCardResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new())
            : ApiResponse<PagedResult<LeaveApplicationCardResponse>>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<LeaveApplicationCardResponse?>> GetLeaveApplicationCardByNoAsync(string applicationNo)
    {
        var apiResponse = await _serviceManager.LeaveApplicationCardService.GetLeaveApplicationCardByNoAsync(applicationNo);
        return apiResponse.Successful
            ? ApiResponse<LeaveApplicationCardResponse?>.Success(apiResponse.Message!, apiResponse.Data)
            : ApiResponse<LeaveApplicationCardResponse?>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<PagedResult<LeaveApplicationCardResponse>>> SearchLeaveApplicationCardsAsync(LeaveApplicationCardFilter filter)
    {
        var apiResponse = await _serviceManager.LeaveApplicationCardService.SearchLeaveApplicationCardsAsync(filter);
        return apiResponse.Successful
            ? ApiResponse<PagedResult<LeaveApplicationCardResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new())
            : ApiResponse<PagedResult<LeaveApplicationCardResponse>>.Failure(apiResponse.Message!);
    }

    #endregion
}
