using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeavePlannerLineService
{
    // Read operations
    Task<AppResponse<List<LeavePlannerLine>>> GetLeavePlannerLinesAsync();
    Task<AppResponse<LeavePlannerLine?>> GetLeavePlannerLineAsync(string employeeNo, string leaveType);
    Task<AppResponse<LeavePlannerLine?>> GetLeavePlannerLineByRecIdAsync(string recId);
    Task<AppResponse<List<LeavePlannerLine>>> SearchLeavePlannerLinesAsync(LeavePlannerLinesFilter filter);

    // Create operations
    Task<AppResponse<LeavePlannerLine>> CreateLeavePlannerLineAsync(LeavePlannerLine request);
    Task<AppResponse<List<LeavePlannerLine>>> CreateMultipleLeavePlannerLinesAsync(List<LeavePlannerLine> requests);

    // Update operations
    Task<AppResponse<LeavePlannerLine>> UpdateLeavePlannerLineAsync(LeavePlannerLine request);
    Task<AppResponse<List<LeavePlannerLine>>> UpdateMultipleLeavePlannerLinesAsync(List<LeavePlannerLine> requests);

    // Delete operations
    Task<AppResponse<bool>> DeleteLeavePlannerLineAsync(string key);

    // Utility operations
    Task<AppResponse<string?>> GetLeavePlannerLineRecIdFromKeyAsync(string key);
    Task<AppResponse<bool>> IsLeavePlannerLineUpdatedAsync(string key);

}
