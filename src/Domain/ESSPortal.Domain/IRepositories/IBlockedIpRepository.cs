using ESSPortal.Domain.Entities;

namespace ESSPortal.Domain.IRepositories;
public interface IBlockedIpRepository : IBaseRepository<BlockedIp>
{
    Task<BlockedIp?> GetByIpAddressAsync(string ipAddress);
    Task<List<BlockedIp>> GetActiveBlockedIpsAsync();
    Task<List<BlockedIp>> GetExpiredBlocksAsync();
}
