using ESSPortal.Domain.Entities;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;

using Microsoft.EntityFrameworkCore;

namespace ESSPortal.Persistence.SQLServer.Implementations.Repositories;
internal sealed class UserProfileRepository : BaseRepository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(DBContext context) : base(context)
    {
    }

    public async Task<UserProfile> CreateOrUpdateAsync(string userId, UserProfile profile)
    {
        var existing = await GetByUserIdAsync(userId);

        if (existing == null)
        {
            profile.UserId = userId;
            profile.CreatedAt = DateTimeOffset.UtcNow;
            profile.UpdatedAt = DateTimeOffset.UtcNow;

            await CreateAsync(profile);
        }
        else
        {
            // Update existing profile
            existing.CountryRegionCode = profile.CountryRegionCode;
            existing.PhysicalAddress = profile.PhysicalAddress;
            existing.TelephoneNo = profile.TelephoneNo;
            existing.MobileNo = profile.MobileNo;
            existing.PostalAddress = profile.PostalAddress;
            existing.PostCode = profile.PostCode;
            existing.City = profile.City;
            existing.ContactEMailAddress = profile.ContactEMailAddress;
            existing.BankAccountType = profile.BankAccountType;
            existing.KBABankCode = profile.KBABankCode;
            existing.KBABranchCode = profile.KBABranchCode;
            existing.BankAccountNo = profile.BankAccountNo;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.UpdatedBy = profile.UpdatedBy;

            profile = existing;

            await UpdateAsync(existing);
        }

        return profile;
    }

    public async Task<bool> ExistsByUserIdAsync(string userId)
    {
        return await _context.UserProfiles.AnyAsync(x => x.UserId == userId && !x.IsDeleted);
            
    }

    public async Task<UserProfile?> GetByUserIdAsync(string userId)
    {
        return await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);
    }
}
