using ESSPortal.Domain.Entities;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Security.Claims;

namespace ESSPortal.Persistence.SQLServer.DataContext;
public class DBContext : IdentityDbContext<AppUser>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public DBContext(DbContextOptions<DBContext> options, IHttpContextAccessor? httpContextAccessor) : base(options) 
    { 
        _httpContextAccessor = httpContextAccessor;


    }



    #region Sets

    public DbSet<BlockedIp> BlockedIps { get; set; }
    public DbSet<IpSecurityEvent> IpSecurityEvents { get; set; }
    public DbSet<IpWhitelist> IpWhitelists { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<TempTotpSecret> TempTotpSecrets { get; set; }
    public DbSet<Upload> Uploads { get; set; }
    public DbSet<UserBackupCode> UserBackupCodes { get; set; }
    public DbSet<UserDevice> UserDevices { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<UserTotpSecret> UserTotpSecrets { get; set; }


    #endregion


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DBContext).Assembly);

        ConfigureIdentityTables(modelBuilder);

    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        // Customize Identity table names if desired
        
        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        // Add indexes to Identity tables for better performance
        builder.Entity<IdentityUserClaim<string>>()
               .HasIndex(uc => uc.UserId)
               .HasDatabaseName("IX_UserClaims_UserId");

        builder.Entity<IdentityUserLogin<string>>()
               .HasIndex(ul => ul.UserId)
               .HasDatabaseName("IX_UserLogins_UserId");

        builder.Entity<IdentityUserRole<string>>()
               .HasIndex(ur => ur.UserId)
               .HasDatabaseName("IX_UserRoles_UserId");

        builder.Entity<IdentityUserRole<string>>()
               .HasIndex(ur => ur.RoleId)
               .HasDatabaseName("IX_UserRoles_RoleId");

        builder.Entity<IdentityUserToken<string>>()
               .HasIndex(ut => ut.UserId)
               .HasDatabaseName("IX_UserTokens_UserId");

        builder.Entity<IdentityRoleClaim<string>>()
               .HasIndex(rc => rc.RoleId)
               .HasDatabaseName("IX_RoleClaims_RoleId");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }
        catch (DbUpdateException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<AppUser>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var currentUserId = GetCurrentUserId();

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                entry.Entity.CreatedBy = currentUserId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedBy = currentUserId;
            }
        }

        var userSessionEntries = ChangeTracker.Entries<UserSession>()
        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in userSessionEntries)
        {
            if (entry.State == EntityState.Added)
            {
                // Only override if CreatedAt wasn't explicitly set
                if (entry.Entity.CreatedAt == default(DateTimeOffset) ||
                    entry.Entity.CreatedAt == DateTimeOffset.MinValue)
                {
                    var now = DateTimeOffset.UtcNow;
                    entry.Entity.CreatedAt = now;

                    // Ensure LastAccessedAt is not before CreatedAt
                    if (entry.Entity.LastAccessedAt < now)
                    {
                        entry.Entity.LastAccessedAt = now;
                    }
                }
                else
                {
                    // CreatedAt was explicitly set, ensure LastAccessedAt respects constraint
                    if (entry.Entity.LastAccessedAt < entry.Entity.CreatedAt)
                    {
                        entry.Entity.LastAccessedAt = entry.Entity.CreatedAt;
                    }
                }

                entry.Entity.CreatedBy = GetCurrentUserId();
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedBy = GetCurrentUserId();
            }
        }

        var refreshTokenEntries = ChangeTracker.Entries<RefreshToken>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in refreshTokenEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
            }
        }

        var deviceEntries = ChangeTracker.Entries<UserDevice>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in deviceEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
            }

            if (entry.State == EntityState.Modified && entry.Entity.IsTrusted)
            {
                entry.Entity.LastUsedAt = DateTimeOffset.UtcNow;
            }
        }

        var profileEntries = ChangeTracker.Entries<Profile>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in profileEntries)
        {
            if (entry.State == EntityState.Added && !entry.Entity.DateCreated.HasValue)
            {
                entry.Entity.DateCreated = DateTimeOffset.UtcNow;
            }
        }

    }

    private string? GetCurrentUserId()
    {
        return _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private static async Task TryRollbackAsync(DbTransaction transaction, CancellationToken ct)
    {
        try
        {
            // Check if transaction is still active
            if (transaction.Connection != null && transaction.Connection.State == ConnectionState.Open)
            {
                await transaction.RollbackAsync(ct);
            }
        }
        catch (InvalidOperationException)
        {
            // Transaction was already completed (committed/rolled back)
            // No action needed
        }
    }

    private static async Task DisposeTransactionSilentlyAsync(DbTransaction transaction)
    {
        try
        {
            await transaction.DisposeAsync();
        }
        catch
        {
            // Suppress disposal errors
            // This is a silent disposal, so we don't log or throw
        }
    }





}
