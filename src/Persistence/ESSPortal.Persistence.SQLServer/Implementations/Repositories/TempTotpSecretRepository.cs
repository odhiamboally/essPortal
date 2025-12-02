using ESSPortal.Domain.Entities;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;
using Microsoft.EntityFrameworkCore;

namespace ESSPortal.Persistence.SQLServer.Implementations.Repositories;
internal sealed class TempTotpSecretRepository : BaseRepository<TempTotpSecret>, ITempTotpSecretRepository
{
    public TempTotpSecretRepository(DBContext context) : base(context)
    {
    }

    public async Task<TempTotpSecret?> GetValidTempSecretByUserIdAsync(string userId)
    {
        return await FindByCondition(x => x.UserId == userId &&
                                         !x.IsDeleted &&
                                         x.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

    }

    public async Task<bool> DeleteExpiredSecretsAsync()
    {
        try
        {
            var expiredSecrets = await FindByCondition(x => x.ExpiresAt <= DateTimeOffset.UtcNow && !x.IsDeleted)
                .ToListAsync();

            foreach (var secret in expiredSecrets)
            {
                secret.IsDeleted = true;
                secret.DeletedAt = DateTimeOffset.UtcNow;
                secret.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await UpdateRangeAsync(expiredSecrets);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserTempSecretsAsync(string userId)
    {
        try
        {
            var tempSecret = await FindByCondition(x => x.UserId == userId && !x.IsDeleted).FirstOrDefaultAsync();
            if (tempSecret == null)
                return false;

            tempSecret.IsDeleted = true;
            tempSecret.DeletedAt = DateTimeOffset.UtcNow;
            tempSecret.UpdatedAt = DateTimeOffset.UtcNow;

            await UpdateAsync(tempSecret);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
