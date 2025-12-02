using ESSPortal.Domain.Entities;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;

using Microsoft.EntityFrameworkCore;

namespace ESSPortal.Persistence.SQLServer.Implementations.Repositories;

internal sealed class IpWhitelistRepository : BaseRepository<IpWhitelist>, IIpWhitelistRepository
{
    public IpWhitelistRepository(DBContext context) : base(context)
    {
    }

    public async Task<IpWhitelist?> GetByIpAddressAsync(string ipAddress)
    {
        return await _context.IpWhitelists
            .FirstOrDefaultAsync(w => w.IpAddress == ipAddress && w.IsActive);
    }

    public async Task<List<IpWhitelist>> GetAdminWhitelistAsync()
    {
        return await _context.IpWhitelists
            .Where(w => w.IsActive && w.IsAdminWhitelist)
            .ToListAsync();
    }

    public async Task<List<IpWhitelist>> GetActiveWhitelistAsync()
    {
        return await _context.IpWhitelists
            .Where(w => w.IsActive)
            .ToListAsync();
    }
}
