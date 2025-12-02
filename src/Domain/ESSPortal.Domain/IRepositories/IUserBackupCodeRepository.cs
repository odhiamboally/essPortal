using ESSPortal.Domain.Entities;

namespace ESSPortal.Domain.IRepositories;
public interface IUserBackupCodeRepository : IBaseRepository<UserBackupCode>
{
    Task<List<UserBackupCode>> GetActiveCodesByUserIdAsync(string userId);
    Task<UserBackupCode?> GetUnusedCodeByHashAsync(string userId, string hashedCode);
    Task<bool> MarkCodeAsUsedAsync(string id);
    Task<bool> DeleteUserCodesAsync(string userId);
    Task<int> GetActiveCodesCountAsync(string userId);
}
