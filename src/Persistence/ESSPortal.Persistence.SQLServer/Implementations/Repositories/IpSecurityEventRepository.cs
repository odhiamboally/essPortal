using ESSPortal.Domain.Entities;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;

using Microsoft.EntityFrameworkCore;

namespace ESSPortal.Persistence.SQLServer.Implementations.Repositories;
internal sealed class IpSecurityEventRepository : BaseRepository<IpSecurityEvent>, IIpSecurityEventRepository
{
    public IpSecurityEventRepository(DBContext context) : base(context)
    {
    }

    public async Task DeleteOldEventsAsync(DateTimeOffset cutoffDate)
    {
        var oldEvents = await _context.IpSecurityEvents
            .Where(e => e.CreatedAt < cutoffDate)
            .ToListAsync();

        _context.IpSecurityEvents.RemoveRange(oldEvents);
    }

    public async Task<int> GetFailedAttemptsCountAsync(string ipAddress, DateTimeOffset since)
    {
        return await _context.IpSecurityEvents
            .CountAsync(e => e.IpAddress == ipAddress &&
                           e.Timestamp >= since &&
                           (e.Result.Contains("failed") || e.Result.Contains("blocked")));
    }

    public async Task<List<IpSecurityEvent>> GetRecentEventsByIpAndOperationsAsync(string ipAddress, DateTimeOffset since, string[] operations)
    {
        return await _context.IpSecurityEvents
            .Where(e => e.IpAddress == ipAddress &&
                       e.Timestamp >= since &&
                       operations.Contains(e.Operation))
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<List<IpSecurityEvent>> GetRecentEventsByIpAsync(string ipAddress, DateTimeOffset since)
    {
        return await _context.IpSecurityEvents
            .Where(e => e.IpAddress == ipAddress && e.Timestamp >= since)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<List<IpSecurityEvent>> GetSuspiciousActivityAsync(DateTimeOffset since)
    {
        return await _context.IpSecurityEvents
            .Where(e => e.Timestamp >= since &&
                       (e.Result.Contains("failed") || e.Result.Contains("blocked")))
            .GroupBy(e => e.IpAddress)
            .Where(g => g.Count() >= 3) // 3 or more failed attempts
            .SelectMany(g => g.OrderByDescending(e => e.Timestamp))
            .ToListAsync();
    }
}
