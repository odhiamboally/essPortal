using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;

namespace ESSPortal.Application.Contracts.Implementations.Common;
internal sealed class ServiceManagerCopy : IServiceManager
{
    public ICacheService CacheService { get; }
    public IAuthService AuthService { get; }
    public IDashboardService DashboardService { get; }
    public IProfileService ProfileService { get; }
    public ITwoFactorService TwoFactorService { get; }
    public ISessionManagementService SessionManagementService { get; }
    public IEmailService EmailService { get; }
    public IFileService FileService { get; }
    public INavisionService NavisionService { get; }
    public IEmployeeService EmployeeService { get; }
    public ILeaveService LeaveService { get; }
    public ILeaveApplicationCardService LeaveApplicationCardService { get; }
    public ILeaveApplicationListService LeaveApplicationListService { get; }
    public ILeaveRelieversService LeaveRelieversService { get; }
    public ILeaveStatisticsFactboxService LeaveStatisticsFactboxService { get; }
    public ILeaveTypesService LeaveTypeService { get; }
    public IApprovedLeaveService ApprovedLeaveService { get; }
    public IPayrollService PayrollService { get; }
    public IPayloadEncryptionService PayloadEncryptionService { get; }





    public ServiceManagerCopy(

        ICacheService cacheService,
        IAuthService accountService,
        IDashboardService dashboardService,
        IProfileService profileService,
        ITwoFactorService twoFactorService,
        ISessionManagementService sessionManagementService,
        IEmailService emailService,
        IFileService fileService,
        INavisionService navisionService,
        IEmployeeService employeeService,
        ILeaveService leaveService,
        ILeaveApplicationCardService leaveApplicationCardService,
        ILeaveApplicationListService leaveApplicationListService,
        ILeaveRelieversService leaveRelieversService,
        ILeaveStatisticsFactboxService leaveStatisticsFactboxService,
        ILeaveTypesService leaveTypeService,
        IApprovedLeaveService approvedLeaveService,
        IPayrollService payrollService,
        IPayloadEncryptionService payloadEncryptionService





        )
    {
        CacheService = cacheService;
        AuthService = accountService;
        DashboardService = dashboardService;
        ProfileService = profileService;
        TwoFactorService = twoFactorService;
        SessionManagementService = sessionManagementService;
        EmailService = emailService;
        FileService = fileService;
        NavisionService = navisionService;
        EmployeeService = employeeService;
        LeaveService = leaveService;
        LeaveApplicationCardService = leaveApplicationCardService;
        LeaveApplicationListService = leaveApplicationListService;
        LeaveRelieversService = leaveRelieversService;
        LeaveStatisticsFactboxService = leaveStatisticsFactboxService;
        LeaveTypeService = leaveTypeService;
        ApprovedLeaveService = approvedLeaveService;

        PayrollService = payrollService;
        PayloadEncryptionService = payloadEncryptionService;



    }
}
