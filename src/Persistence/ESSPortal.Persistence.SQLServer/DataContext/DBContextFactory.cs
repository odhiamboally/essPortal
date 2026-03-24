using ESSPortal.Persistence.SQLServer.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Reflection;

namespace ESSPortal.Persistence.SQLServer.DataContext;
public class DBContextFactory() : IDesignTimeDbContextFactory<DBContext>
{
    public DBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Production.json", optional: false)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<DBContext>();
        var connectionString = configuration.GetConnectionString("EssPortal");

        optionsBuilder.UseSqlServer(connectionString, PersistenceExtensions.ConfigureSqlOptions);

        return new DBContext(optionsBuilder.Options);
    }
}
