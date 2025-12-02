using ESSPortal.Domain.Entities;

namespace ESSPortal.Domain.IRepositories;
public interface IUserProfileRepository : IBaseRepository<UserProfile>
{
    Task<UserProfile?> GetByUserIdAsync(string userId);
    Task<bool> ExistsByUserIdAsync(string userId);
    Task<UserProfile> CreateOrUpdateAsync(string userId, UserProfile profile);
}