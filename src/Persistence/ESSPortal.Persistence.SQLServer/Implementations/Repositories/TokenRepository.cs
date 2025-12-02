using ESSPortal.Domain.Entities;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;
using ESSPortal.Persistence.SQLServer.Implementations.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESSPortal.Infrastructure.Implementations.Repositories;
internal sealed class TokenRepository : BaseRepository<RefreshToken>, ITokenRepository
{
    private readonly ILogger<TokenRepository> _logger;
    public TokenRepository(DBContext context, ILogger<TokenRepository> logger) : base(context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        try
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Added refresh token for user {UserId}", refreshToken.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add refresh token for user {UserId}", refreshToken.UserId);
            throw;
        }
    }

    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {

        var now = DateTimeOffset.UtcNow;

        return await FindByCondition(token =>
            token.UserId == userId &&
            !token.RevokedAt.HasValue &&           // Not revoked
            now < token.ExpiresAt &&               // Not expired
            !token.UsedAt.HasValue)                // Not used
            .ToListAsync();

    }

    public async Task<RefreshToken?> GetByTokenAndUserAsync(string token, string userId)
    {
        return await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.Token == token && t.UserId == userId);
    }

    public async Task<RefreshToken?> GetTokenAsync(string token)
    {
        return await FindByCondition(t => t.Token == token).AsNoTracking().FirstOrDefaultAsync();
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, string userId)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.UserId == userId);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<List<RefreshToken>> GetUserTokensAsync(string userId, int limit = 10)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<RefreshToken>> GetExpiredTokensAsync(int daysOld = 30)
    {
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-daysOld);

        return await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTimeOffset.UtcNow && rt.CreatedAt < cutoffDate)
            .ToListAsync();
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeTokensAsync(List<RefreshToken> tokens, string reason, string? revokedByIp = null)
    {
        if (!tokens.Any()) return;
        if (string.IsNullOrWhiteSpace(revokedByIp))
        {
            revokedByIp = "Unknown";
        }

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            token.RevokedReason = reason;
            token.RevokedByIp = revokedByIp;
            token.UpdatedAt = DateTimeOffset.UtcNow;
            await UpdateAsync(token);
        }

        _logger.LogInformation("Revoked {Count} tokens during logout", tokens.Count);
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string reason, string? revokedByIp = null)
    {
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        refreshToken.RevokedReason = reason;
        refreshToken.RevokedByIp = revokedByIp;

        await UpdateRefreshTokenAsync(refreshToken);
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string reason)
    {
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        refreshToken.RevokedReason = reason;

        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Revoked refresh token for user {UserId}: {Reason}",
            refreshToken.UserId, reason);
    }

    public async Task RevokeAllUserTokensAsync(string userId, string reason, string? revokedByIp = null)
    {
        var activeTokens = await GetActiveTokensByUserIdAsync(userId);

        if (!activeTokens.Any())
        {
            _logger.LogDebug("No active tokens found for user {UserId}", userId);
            return;
        }

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            token.RevokedReason = reason;
            token.RevokedByIp = revokedByIp;
            token.UpdatedAt = DateTimeOffset.UtcNow;

        }

        _context.UpdateRange(activeTokens);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Revoked {Count} tokens for user {UserId}: {Reason}", activeTokens.Count, userId, reason);

    }

    public async Task<bool> IsTokenActiveAsync(string token)
    {
        var refreshToken = await FindByCondition(t =>
            t.Token == token &&
            t.IsActive &&
            !t.IsExpired &&
            !t.IsRevoked &&
            !t.IsUsed)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return refreshToken != null;
    }

    public async Task MarkTokenAsUsedAsync(RefreshToken refreshToken)
    {
        refreshToken.UsedAt = DateTimeOffset.UtcNow;

        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Marked token as used for user {UserId}", refreshToken.UserId);
    }

    public async Task CleanupExpiredTokensAsync(string? userId = null)
    {
        // Clean up tokens older than 30 days
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-30);
        var now = DateTimeOffset.UtcNow;

        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId &&
                        (rt.ExpiresAt < now ||
                         rt.CreatedAt < cutoffDate ||
                         (rt.IsRevoked && rt.RevokedAt < cutoffDate)))
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            expiredTokens = expiredTokens.Where(t => t.UserId == userId).ToList();
        }

        if (expiredTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired tokens for user {UserId}",
                expiredTokens.Count, userId);
        }
    }

    public async Task PerformMaintenanceAsync()
    {
        await CleanupExpiredTokensAsync();

        // Log statistics
        await _context.RefreshTokens.CountAsync();
        await _context.RefreshTokens.CountAsync(rt => rt.IsActive);
        await _context.RefreshTokens.CountAsync(rt => rt.IsExpired);
    }


}
