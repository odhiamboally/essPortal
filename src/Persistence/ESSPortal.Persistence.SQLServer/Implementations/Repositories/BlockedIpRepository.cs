using ESSPortal.Domain.Entities;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;

using Microsoft.EntityFrameworkCore;

namespace ESSPortal.Persistence.SQLServer.Implementations.Repositories;


internal sealed class BlockedIpRepository : BaseRepository<BlockedIp>, IBlockedIpRepository
{
    public BlockedIpRepository(DBContext context) : base(context)
    {
    }

    public async Task<BlockedIp?> GetByIpAddressAsync(string ipAddress)
    {
        return await _context.BlockedIps
            .FirstOrDefaultAsync(b => b.IpAddress == ipAddress && b.IsActive);
    }

    public async Task<List<BlockedIp>> GetActiveBlockedIpsAsync()
    {
        return await _context.BlockedIps
            .Where(b => b.IsActive &&
                       (b.ExpiresAt == null || b.ExpiresAt > DateTimeOffset.UtcNow))
            .ToListAsync();
    }

    public async Task<List<BlockedIp>> GetExpiredBlocksAsync()
    {
        return await _context.BlockedIps
            .Where(b => b.IsActive &&
                       b.ExpiresAt.HasValue &&
                       b.ExpiresAt <= DateTimeOffset.UtcNow)
            .ToListAsync();
    }
}
