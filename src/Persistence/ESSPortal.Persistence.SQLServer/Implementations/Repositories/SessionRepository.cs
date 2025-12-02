using ESSPortal.Domain.Entities;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;

using Microsoft.EntityFrameworkCore;

namespace ESSPortal.Persistence.SQLServer.Implementations.Repositories;
internal sealed class SessionRepository : BaseRepository<UserSession>, ISessionRepository
{
    public SessionRepository(DBContext context) : base(context)
    {
    }

    public async Task<List<UserSession>> GetActiveSessionsByUserIdAsync(string userId)
    {
        return await _context.UserSessions
            .Where(s => s.UserId == userId &&
                       s.IsActive &&
                       s.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(s => s.LastAccessedAt)
            .ToListAsync();
    }

    public async Task<UserSession?> GetOldestSessionByUserIdAsync(string userId)
    {
        return await _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<UserSession>> GetExpiredSessionsAsync()
    {
        return await _context.UserSessions
            .Where(s => s.IsActive && s.ExpiresAt <= DateTimeOffset.UtcNow)
            .ToListAsync();
    }

    // public async Task UpdateRangeAsync(List<UserSession> sessions)
    // {
    //     _context.UserSessions.UpdateRange(sessions);
    //     await Task.CompletedTask;
    // }
}
