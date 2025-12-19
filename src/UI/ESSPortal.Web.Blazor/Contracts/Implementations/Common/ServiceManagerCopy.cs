using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.Common;

internal sealed class ServiceManagerCopy : IClientServiceManager
{
    public IAppStateService AppStateService { get; }
    public IAppUserService UserService { get; }
    public IProfileService ProfileService { get; }
    public IFileService FileService { get; }
    public ITwoFactorService TwoFactorService { get; }
    public IDashboardService DashboardService { get; }
    public IPayrollService PayrollService { get; }
    public IAuthService AuthService { get; }
    public IEmployeeService EmployeeService { get; }
    public ILeaveService LeaveService { get; }
    public ILeaveApplicationCardService LeaveApplicationCardService { get; }
    public ILeaveApplicationListService LeaveApplicationListService { get; }
    public ILeaveRelieverService LeaveRelieverService { get; }
    public ILeaveStatisticsFactboxService LeaveStatisticsFactboxService { get; }
    public ILeaveTypeService LeaveTypeService { get; }
    public ICacheService CacheService { get; }
    public IPayloadEncryptionService PayloadEncryptionService { get; }



    public ServiceManagerCopy(IServiceProvider serviceProvider,
        
        IAppStateService appStateService,
        IAppUserService userService,
        IProfileService profileService,
        IFileService fileService,
        ITwoFactorService twoFactorService,
        IDashboardService dashboardService,
        IPayrollService payrollService,
        IAuthService authService,
        IEmployeeService employeeService,
        ILeaveService leaveService,
        ILeaveApplicationCardService leaveApplicationCardService,
        ILeaveApplicationListService leaveApplicationListService,
        ILeaveRelieverService leaveRelieverService,
        ILeaveStatisticsFactboxService leaveStatisticsFactboxService,
        ILeaveTypeService leaveTypeService,
        ICacheService cacheService,
        IPayloadEncryptionService payloadEncryptionService




        )
    {

        AppStateService = appStateService;
        UserService = userService;
        ProfileService = profileService;
        FileService = fileService;
        TwoFactorService = twoFactorService;
        DashboardService = dashboardService;
        PayrollService = payrollService;
        AuthService = authService;
        EmployeeService = employeeService;
        LeaveService = leaveService;
        LeaveApplicationCardService = leaveApplicationCardService;
        LeaveApplicationListService = leaveApplicationListService;
        LeaveRelieverService = leaveRelieverService;
        LeaveStatisticsFactboxService = leaveStatisticsFactboxService;
        LeaveTypeService = leaveTypeService;
        CacheService = cacheService;
        PayloadEncryptionService = payloadEncryptionService;




    }


}
