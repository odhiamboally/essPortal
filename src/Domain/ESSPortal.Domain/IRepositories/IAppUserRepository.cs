using ESSPortal.Domain.Entities;

namespace ESSPortal.Domain.IRepositories;
public interface IAppUserRepository : IBaseRepository<AppUser>
{
    Task<AppUser?> GetUserWithDevicesAsync(string userId);
    Task<AppUser?> GetUserWithTokensAsync(string userId);
    Task<AppUser?> GetUserWithProfileAsync(string userId);
    Task<AppUser?> GetUserByEmployeeNumberAsync(string employeeNumber);
    Task<List<AppUser>> GetActiveUsersAsync();
    Task<List<AppUser>> GetUsersInDepartmentAsync(string department);
    Task<List<AppUser>> GetDirectReportsAsync(string managerId);
    Task<List<AppUser>> GetUsersRequiringPasswordChangeAsync();
    Task<List<AppUser>> GetLockedOutUsersAsync();
    Task<int> GetActiveUserCountAsync();
    Task<List<AppUser>> SearchUsersAsync(string searchTerm);
}
