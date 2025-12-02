using ESSPortal.Application.Contracts.Interfaces.Services;

namespace ESSPortal.Application.Contracts.Interfaces.Common;
public interface IServiceManager
{
    ICacheService CacheService { get; }
    IAuthService AuthService { get; }
    IDashboardService DashboardService { get; }
    IProfileService ProfileService { get; }
    ITwoFactorService TwoFactorService { get; }
    ISessionManagementService SessionManagementService { get; }
    IEmailService EmailService { get; }
    IFileService FileService { get; }
    INavisionService NavisionService { get; }
    IEmployeeService EmployeeService { get; }
    ILeaveService LeaveService { get; }
    ILeaveApplicationCardService LeaveApplicationCardService { get; }
    ILeaveApplicationListService LeaveApplicationListService { get; }
    ILeaveRelieversService LeaveRelieversService { get; }
    ILeaveStatisticsFactboxService LeaveStatisticsFactboxService { get; }
    ILeaveTypesService LeaveTypeService { get; }
    IApprovedLeaveService ApprovedLeaveService { get; }
    IPayrollService PayrollService { get; }
    IPayloadEncryptionService PayloadEncryptionService { get; }



}
