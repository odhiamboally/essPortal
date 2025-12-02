using ESSPortal.Domain.Entities;

namespace ESSPortal.Domain.IRepositories;
public interface ISessionRepository : IBaseRepository<UserSession>
{
    Task<List<UserSession>> GetActiveSessionsByUserIdAsync(string userId);
    Task<UserSession?> GetOldestSessionByUserIdAsync(string userId);
    Task<List<UserSession>> GetExpiredSessionsAsync();
    
}
