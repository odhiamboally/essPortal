using ESSPortal.Domain.Entities;

namespace ESSPortal.Domain.IRepositories;
public interface IIpWhitelistRepository : IBaseRepository<IpWhitelist>
{
    Task<IpWhitelist?> GetByIpAddressAsync(string ipAddress);
    Task<List<IpWhitelist>> GetAdminWhitelistAsync();
    Task<List<IpWhitelist>> GetActiveWhitelistAsync();
}
