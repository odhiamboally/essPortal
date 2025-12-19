
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Common;

public interface IClientServiceManager
{

    IAppStateService AppStateService { get; }
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
    ILeaveRelieverService LeaveRelieverService { get; }
    ILeaveStatisticsFactboxService LeaveStatisticsFactboxService { get; }
    ILeaveTypeService LeaveTypeService { get; }
    ICacheService CacheService { get; }
    IPayloadEncryptionService PayloadEncryptionService { get; }
    
    
    



}
