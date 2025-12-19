using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Common;

internal sealed class ServiceManager : IServiceManager
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Lazy<IAppUserService> _userService;
    private readonly Lazy<IProfileService> _profileService;
    private readonly Lazy<IFileService> _fileService;
    private readonly Lazy<ITwoFactorService> _twoFactorService;
    private readonly Lazy<IDashboardService> _dashboardService;
    private readonly Lazy<IPayrollService> _payrollService;
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IEmployeeService> _employeeService;
    private readonly Lazy<ILeaveService> _leaveService;
    private readonly Lazy<ILeaveApplicationCardService> _leaveApplicationCardService;
    private readonly Lazy<ILeaveApplicationListService> _leaveApplicationListService;
    private readonly Lazy<ILeavePlannerLineService> _leavePlannerLineService;
    private readonly Lazy<ILeaveRelieverService> _leaveRelieverService;
    private readonly Lazy<ILeaveStatisticsFactboxService> _leaveStatisticsFactboxService;
    private readonly Lazy<ILeaveTypeService> _leaveTypeService;
    private readonly Lazy<ICacheService> _cacheService;
    private readonly Lazy<IPayloadEncryptionService> _payloadEncryptionService;

    public ServiceManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _userService = new Lazy<IAppUserService>(() => _serviceProvider.GetRequiredService<IAppUserService>());
        _profileService = new Lazy<IProfileService>(() => _serviceProvider.GetRequiredService<IProfileService>());
        _fileService = new Lazy<IFileService>(() => _serviceProvider.GetRequiredService<IFileService>());
        _twoFactorService = new Lazy<ITwoFactorService>(() => _serviceProvider.GetRequiredService<ITwoFactorService>());
        _dashboardService = new Lazy<IDashboardService>(() => _serviceProvider.GetRequiredService<IDashboardService>());
        _payrollService = new Lazy<IPayrollService>(() => _serviceProvider.GetRequiredService<IPayrollService>());
        _authService = new Lazy<IAuthService>(() => _serviceProvider.GetRequiredService<IAuthService>());
        _employeeService = new Lazy<IEmployeeService>(() => _serviceProvider.GetRequiredService<IEmployeeService>());
        _leaveService = new Lazy<ILeaveService>(() => _serviceProvider.GetRequiredService<ILeaveService>());
        _leaveApplicationCardService = new Lazy<ILeaveApplicationCardService>(() => _serviceProvider.GetRequiredService<ILeaveApplicationCardService>());
        _leaveApplicationListService = new Lazy<ILeaveApplicationListService>(() => _serviceProvider.GetRequiredService<ILeaveApplicationListService>());
        _leavePlannerLineService = new Lazy<ILeavePlannerLineService>(() => _serviceProvider.GetRequiredService<ILeavePlannerLineService>());
        _leaveRelieverService = new Lazy<ILeaveRelieverService>(() => _serviceProvider.GetRequiredService<ILeaveRelieverService>());
        _leaveStatisticsFactboxService = new Lazy<ILeaveStatisticsFactboxService>(() => _serviceProvider.GetRequiredService<ILeaveStatisticsFactboxService>());
        _leaveTypeService = new Lazy<ILeaveTypeService>(() => _serviceProvider.GetRequiredService<ILeaveTypeService>());
        _cacheService = new Lazy<ICacheService>(() => _serviceProvider.GetRequiredService<ICacheService>());
        _payloadEncryptionService = new Lazy<IPayloadEncryptionService>(() => _serviceProvider.GetRequiredService<IPayloadEncryptionService>());
    }

    public IAppUserService UserService => _userService.Value;
    public IProfileService ProfileService => _profileService.Value;
    public IFileService FileService => _fileService.Value;
    public ITwoFactorService TwoFactorService => _twoFactorService.Value;
    public IDashboardService DashboardService => _dashboardService.Value;
    public IPayrollService PayrollService => _payrollService.Value;
    public IAuthService AuthService => _authService.Value;
    public IEmployeeService EmployeeService => _employeeService.Value;
    public ILeaveService LeaveService => _leaveService.Value;
    public ILeaveApplicationCardService LeaveApplicationCardService => _leaveApplicationCardService.Value;
    public ILeaveApplicationListService LeaveApplicationListService => _leaveApplicationListService.Value;
    public ILeavePlannerLineService LeavePlannerLineService => _leavePlannerLineService.Value;
    public ILeaveRelieverService LeaveRelieverService => _leaveRelieverService.Value;
    public ILeaveStatisticsFactboxService LeaveStatisticsFactboxService => _leaveStatisticsFactboxService.Value;
    public ILeaveTypeService LeaveTypeService => _leaveTypeService.Value;
    public ICacheService CacheService => _cacheService.Value;
    public IPayloadEncryptionService PayloadEncryptionService => _payloadEncryptionService.Value;


}
