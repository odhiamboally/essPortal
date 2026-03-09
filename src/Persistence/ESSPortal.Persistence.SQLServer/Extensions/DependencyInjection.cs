using ESSPortal.Domain.Entities;
using ESSPortal.Domain.Interfaces;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Infrastructure.Implementations.Repositories;
using ESSPortal.Persistence.SQLServer.DataContext;
using ESSPortal.Persistence.SQLServer.Implementations.Intefaces;
using ESSPortal.Persistence.SQLServer.Implementations.Repositories;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Reflection;

namespace ESSPortal.Persistence.SQLServer.Extensions;


public static class DependencyInjection
{
    public static void AddLocalPersistenceDI(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureLocalPersistence(services, configuration);
        AddServices(services);
    }

    public static void AddPersistenceDI(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureUNSaccoPersistence(services, configuration);
        AddServices(services);
    }

    private static void ConfigureLocalPersistence(IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var ConnString = configuration.GetConnectionString("LocalConn");
            services.AddDbContext<DBContext>(options => options.UseSqlServer(ConnString, PersistenceExtensions.ConfigureSqlOptions));

        }
        catch (Exception)
        { 
            throw;
        }
    }

    private static void ConfigureUNSaccoPersistence(IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var ConnString = configuration.GetConnectionString("EssPortal");
            services.AddDbContext<DBContext>(options => options.UseSqlServer(ConnString, PersistenceExtensions.ConfigureSqlOptions));

        }
        catch (Exception)
        {
            throw;
        }
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IUploadRepository, UploadRepository>();
        services.AddScoped<IIpSecurityEventRepository, IpSecurityEventRepository>();
        services.AddScoped<IBlockedIpRepository, BlockedIpRepository>();
        services.AddScoped<IIpWhitelistRepository, IpWhitelistRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IUserBackupCodeRepository, UserBackupCodeRepository>();
        services.AddScoped<ITempTotpSecretRepository, TempTotpSecretRepository>();
        services.AddScoped<IUserTotpSecretRepository, UserTotpSecretRepository>();

        return services;
    }

    public static async Task<IServiceProvider> SeedDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<DbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        try
        {
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetService<ILogger<DbContext>>();
            logger?.LogError(ex, "An error occurred while seeding the database");
            throw;
        }

        return serviceProvider;
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Manager", "Employee"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<AppUser> userManager)
    {
        // Create default admin user if it doesn't exist
        var adminEmail = "admin@yourcompany.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                EmployeeNumber = "ADMIN001",
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true,
                TwoFactorEnabled = false // You might want to enable this later
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123!"); // Change this password!

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");

                // Add some claims
                await userManager.AddClaimAsync(adminUser, new System.Security.Claims.Claim("permission", "admin.all"));
                await userManager.AddClaimAsync(adminUser, new System.Security.Claims.Claim("department", "IT"));
            }
        }
    }
}
