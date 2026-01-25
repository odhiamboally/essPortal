

using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Common;

public interface IClientServiceManager
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
    ILeaveRelieverService LeaveRelieverService { get; }
    ILeaveStatisticsFactboxService LeaveStatisticsFactboxService { get; }
    ILeaveTypeService LeaveTypeService { get; }
    ICacheService CacheService { get; }
    IPayloadEncryptionService PayloadEncryptionService { get; }
    
    
    



}
