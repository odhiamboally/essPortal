using ESSPortal.Domain.Entities;

namespace ESSPortal.Domain.IRepositories;
public interface IUserTotpSecretRepository : IBaseRepository<UserTotpSecret>
{
    Task<UserTotpSecret?> GetByUserIdAsync(string userId);
    Task<UserTotpSecret?> GetActiveSecretByUserIdAsync(string userId);
    Task<bool> DeactivateUserSecretsAsync(string userId);
}
