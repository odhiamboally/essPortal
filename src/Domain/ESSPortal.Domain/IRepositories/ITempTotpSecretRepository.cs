using ESSPortal.Domain.Entities;

namespace ESSPortal.Domain.IRepositories;
public interface ITempTotpSecretRepository : IBaseRepository<TempTotpSecret>
{
    Task<TempTotpSecret?> GetValidTempSecretByUserIdAsync(string userId);
    Task<bool> DeleteExpiredSecretsAsync();
    Task<bool> DeleteUserTempSecretsAsync(string userId);
}