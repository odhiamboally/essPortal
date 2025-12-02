using ESSPortal.Domain.Entities;

namespace ESSPortal.Domain.IRepositories;
public interface IIpSecurityEventRepository : IBaseRepository<IpSecurityEvent>
{
    Task<List<IpSecurityEvent>> GetRecentEventsByIpAsync(string ipAddress, DateTimeOffset since);
    Task<List<IpSecurityEvent>> GetRecentEventsByIpAndOperationsAsync(string ipAddress, DateTimeOffset since, string[] operations);
    Task<int> GetFailedAttemptsCountAsync(string ipAddress, DateTimeOffset since);
    Task<List<IpSecurityEvent>> GetSuspiciousActivityAsync(DateTimeOffset since);
    Task DeleteOldEventsAsync(DateTimeOffset cutoffDate);
}
