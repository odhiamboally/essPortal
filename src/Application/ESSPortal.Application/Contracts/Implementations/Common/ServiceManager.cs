using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;

using Microsoft.Extensions.DependencyInjection;

namespace ESSPortal.Application.Contracts.Implementations.Common;
internal sealed class ServiceManager : IServiceManager
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Lazy<ICacheService> _cacheService;
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IDashboardService> _dashboardService;
    private readonly Lazy<IProfileService> _profileService;
    private readonly Lazy<ITwoFactorService> _twoFactorService;
    private readonly Lazy<ISessionManagementService> _sessionManagementService;
    private readonly Lazy<IEmailService> _emailService;
    private readonly Lazy<IFileService> _fileService;
    private readonly Lazy<INavisionService> _navisionService;
    private readonly Lazy<IEmployeeService> _employeeService;
    private readonly Lazy<ILeaveService> _leaveService;
    private readonly Lazy<ILeaveApplicationCardService> _leaveApplicationCardService;
    private readonly Lazy<ILeaveApplicationListService> _leaveApplicationListService;
    private readonly Lazy<ILeaveRelieversService> _leaveRelieversService;
    private readonly Lazy<ILeaveStatisticsFactboxService> _leaveStatisticsFactboxService;
    private readonly Lazy<ILeaveTypesService> _leaveTypeService;
    private readonly Lazy<IApprovedLeaveService> _approvedLeaveService;
    private readonly Lazy<IPayrollService> _payrollService;
    private readonly Lazy<IPayloadEncryptionService> _payloadEncryptionService;

    public ServiceManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _cacheService = new Lazy<ICacheService>(() => _serviceProvider.GetRequiredService<ICacheService>());
        _authService = new Lazy<IAuthService>(() => _serviceProvider.GetRequiredService<IAuthService>());
        _dashboardService = new Lazy<IDashboardService>(() => _serviceProvider.GetRequiredService<IDashboardService>());
        _profileService = new Lazy<IProfileService>(() => _serviceProvider.GetRequiredService<IProfileService>());
        _twoFactorService = new Lazy<ITwoFactorService>(() => _serviceProvider.GetRequiredService<ITwoFactorService>());
        _sessionManagementService = new Lazy<ISessionManagementService>(() => _serviceProvider.GetRequiredService<ISessionManagementService>());
        _emailService = new Lazy<IEmailService>(() => _serviceProvider.GetRequiredService<IEmailService>());
        _fileService = new Lazy<IFileService>(() => _serviceProvider.GetRequiredService<IFileService>());
        _navisionService = new Lazy<INavisionService>(() => _serviceProvider.GetRequiredService<INavisionService>());
        _employeeService = new Lazy<IEmployeeService>(() => _serviceProvider.GetRequiredService<IEmployeeService>());
        _leaveService = new Lazy<ILeaveService>(() => _serviceProvider.GetRequiredService<ILeaveService>());
        _leaveApplicationCardService = new Lazy<ILeaveApplicationCardService>(() => _serviceProvider.GetRequiredService<ILeaveApplicationCardService>());
        _leaveApplicationListService = new Lazy<ILeaveApplicationListService>(() => _serviceProvider.GetRequiredService<ILeaveApplicationListService>());
        _leaveRelieversService = new Lazy<ILeaveRelieversService>(() => _serviceProvider.GetRequiredService<ILeaveRelieversService>());
        _leaveStatisticsFactboxService = new Lazy<ILeaveStatisticsFactboxService>(() => _serviceProvider.GetRequiredService<ILeaveStatisticsFactboxService>());
        _leaveTypeService = new Lazy<ILeaveTypesService>(() => _serviceProvider.GetRequiredService<ILeaveTypesService>());
        _approvedLeaveService = new Lazy<IApprovedLeaveService>(() => _serviceProvider.GetRequiredService<IApprovedLeaveService>());
        _payrollService = new Lazy<IPayrollService>(() => _serviceProvider.GetRequiredService<IPayrollService>());
        _payloadEncryptionService = new Lazy<IPayloadEncryptionService>(() => _serviceProvider.GetRequiredService<IPayloadEncryptionService>());
    }

    public ICacheService CacheService => _cacheService.Value;
    public IAuthService AuthService => _authService.Value;
    public IDashboardService DashboardService => _dashboardService.Value;
    public IProfileService ProfileService => _profileService.Value;
    public ITwoFactorService TwoFactorService => _twoFactorService.Value;
    public ISessionManagementService SessionManagementService => _sessionManagementService.Value;
    public IEmailService EmailService => _emailService.Value;
    public IFileService FileService => _fileService.Value;
    public INavisionService NavisionService => _navisionService.Value;
    public IEmployeeService EmployeeService => _employeeService.Value;
    public ILeaveService LeaveService => _leaveService.Value;
    public ILeaveApplicationCardService LeaveApplicationCardService => _leaveApplicationCardService.Value;
    public ILeaveApplicationListService LeaveApplicationListService => _leaveApplicationListService.Value;
    public ILeaveRelieversService LeaveRelieversService => _leaveRelieversService.Value;
    public ILeaveStatisticsFactboxService LeaveStatisticsFactboxService => _leaveStatisticsFactboxService.Value;
    public ILeaveTypesService LeaveTypeService => _leaveTypeService.Value;
    public IApprovedLeaveService ApprovedLeaveService => _approvedLeaveService.Value;
    public IPayrollService PayrollService => _payrollService.Value;
    public IPayloadEncryptionService PayloadEncryptionService => _payloadEncryptionService.Value;
}
