
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Common;

public interface IServiceManager
{

    IAppUserService UserService { get; }
    IProfileService ProfileService { get; }
    IFileService FileService { get; }
    ITwoFactorService TwoFactorService { get; }
    IDashboardService DashboardService { get; }
    IPayrollService PayrollService { get; }
    IAuthService AuthService { get; }
    IEmployeeService EmployeeService { get; }
    ILeaveService LeaveService { get; }
    ILeaveApplicationCardService LeaveApplicationCardService { get; }
    ILeaveApplicationListService LeaveApplicationListService { get; }
    ILeavePlannerLineService LeavePlannerLineService { get; }
    ILeaveRelieverService LeaveRelieverService { get; }
    ILeaveStatisticsFactboxService LeaveStatisticsFactboxService { get; }
    ILeaveTypeService LeaveTypeService { get; }
    ICacheService CacheService { get; }
    IPayloadEncryptionService PayloadEncryptionService { get; }
    
    
    



}
