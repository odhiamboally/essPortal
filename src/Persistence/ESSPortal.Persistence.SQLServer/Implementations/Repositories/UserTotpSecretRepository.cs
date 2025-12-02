using ESSPortal.Domain.Entities;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;

using Microsoft.EntityFrameworkCore;

namespace ESSPortal.Persistence.SQLServer.Implementations.Repositories;
internal sealed class UserTotpSecretRepository : BaseRepository<UserTotpSecret>, IUserTotpSecretRepository
{
    public UserTotpSecretRepository(DBContext context) : base(context)
    {
    }

    public async Task<UserTotpSecret?> GetByUserIdAsync(string userId)
    {
        return await FindByCondition(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<UserTotpSecret?> GetActiveSecretByUserIdAsync(string userId)
    {
        return await FindByCondition(x =>
                x.UserId == userId &&
                x.IsActive &&
                !x.IsDeleted &&
                (x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow))
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeactivateUserSecretsAsync(string userId)
    {
        try
        {
            var secrets = await FindByCondition(x => x.UserId == userId && x.IsActive)
                .ToListAsync();

            foreach (var secret in secrets)
            {
                secret.IsActive = false;
                secret.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await UpdateRangeAsync(secrets);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

