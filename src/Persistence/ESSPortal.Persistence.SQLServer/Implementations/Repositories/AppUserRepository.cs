using ESSPortal.Domain.Entities;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;
using ESSPortal.Persistence.SQLServer.Implementations.Repositories;

using Microsoft.EntityFrameworkCore;

namespace ESSPortal.Infrastructure.Implementations.Repositories;

internal sealed class AppUserRepository : BaseRepository<AppUser>, IAppUserRepository
{

    public AppUserRepository(DBContext context) : base(context)
    {
    }

    public async Task<int> GetActiveUserCountAsync()
    {
        return await _context.Users
            .CountAsync(u => u.IsActive && !u.IsDeleted);
    }

    public async Task<List<AppUser>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive && !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<AppUser>> GetDirectReportsAsync(string managerId)
    {
        return await _context.Users
            .Where(u => u.ManagerId == managerId && u.IsActive && !u.IsDeleted)
            .Include(u => u.Profile)
            .ToListAsync();
    }

    public async Task<List<AppUser>> GetLockedOutUsersAsync()
    {
        return await _context.Users
            .Where(u => u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow)
            .ToListAsync();
    }

    public async Task<AppUser?> GetUserByEmployeeNumberAsync(string employeeNumber)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.EmployeeNumber == employeeNumber);
    }

    public async Task<List<AppUser>> GetUsersInDepartmentAsync(string department)
    {
        return await _context.Users
            .Where(u => u.Department == department && u.IsActive && !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<AppUser>> GetUsersRequiringPasswordChangeAsync()
    {
        var ninetyDaysAgo = DateTimeOffset.UtcNow.AddDays(-90);

        return await _context.Users
            .Where(u => u.IsActive && !u.IsDeleted)
            .Where(u => u.RequirePasswordChange ||
                       (u.PasswordLastChanged.HasValue && u.PasswordLastChanged.Value < ninetyDaysAgo) ||
                       (!u.PasswordLastChanged.HasValue && u.CreatedAt < ninetyDaysAgo))
            .ToListAsync();
    }

    public async Task<AppUser?> GetUserWithDevicesAsync(string userId)
    {
        return await _context.Users
            .Include(u => u.TrustedDevices)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<AppUser?> GetUserWithProfileAsync(string userId)
    {
        return await _context.Users
            .Include(u => u.Profile)
            .ThenInclude(p => p!.Uploads)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<AppUser?> GetUserWithTokensAsync(string userId)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens.Where(rt => rt.IsActive))
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<AppUser>> SearchUsersAsync(string searchTerm)
    {
        var term = searchTerm.ToLowerInvariant();

        return await _context.Users
            .Where(u => u.IsActive && !u.IsDeleted)
            .Where(u => u.FirstName!.ToLowerInvariant().Contains(term) ||
                       u.LastName!.ToLowerInvariant().Contains(term) ||
                       u.Email!.ToLowerInvariant().Contains(term) ||
                       u.EmployeeNumber!.ToLowerInvariant().Contains(term))
            .Take(50) 
            .ToListAsync();
    }
}
