using ESSPortal.Domain.Entities;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;

using Microsoft.EntityFrameworkCore;

namespace ESSPortal.Persistence.SQLServer.Implementations.Repositories;
internal sealed class UserBackupCodeRepository : BaseRepository<UserBackupCode>, IUserBackupCodeRepository
{
    public UserBackupCodeRepository(DBContext context) : base(context)
    {
    }

    public async Task<List<UserBackupCode>> GetActiveCodesByUserIdAsync(string userId)
    {
        return await FindByCondition(x => x.UserId == userId &&
                                         !x.IsUsed &&
                                         !x.IsDeleted &&
                                         x.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserBackupCode?> GetUnusedCodeByHashAsync(string userId, string hashedCode)
    {
        return await FindByCondition(x => x.UserId == userId &&
                                         x.HashedCode == hashedCode &&
                                         !x.IsUsed &&
                                         !x.IsDeleted &&
                                         x.ExpiresAt > DateTimeOffset.UtcNow)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> MarkCodeAsUsedAsync(string id)
    {
        try
        {
            var code = await FindByCondition(x => x.Id == id).FirstOrDefaultAsync();
            if (code == null) return false;

            code.IsUsed = true;
            code.UsedAt = DateTimeOffset.UtcNow;
            code.UpdatedAt = DateTimeOffset.UtcNow;

            await UpdateAsync(code);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserCodesAsync(string userId)
    {
        try
        {
            var codes = await FindByCondition(x => x.UserId == userId).ToListAsync();

            foreach (var code in codes)
            {
                code.IsDeleted = true;
                code.DeletedAt = DateTimeOffset.UtcNow;
                code.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await UpdateRangeAsync(codes);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<int> GetActiveCodesCountAsync(string userId)
    {
        return await FindByCondition(x => x.UserId == userId &&
                                         !x.IsUsed &&
                                         !x.IsDeleted &&
                                         x.ExpiresAt > DateTimeOffset.UtcNow)
            .CountAsync();
    }
}

