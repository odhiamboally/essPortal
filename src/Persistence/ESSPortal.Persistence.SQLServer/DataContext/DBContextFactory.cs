using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ESSPortal.Persistence.SQLServer.DataContext;
public class DBContextFactory : IDesignTimeDbContextFactory<DBContext>
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
        //var connectionString = configuration.GetConnectionString("EssPortal");

        optionsBuilder.UseSqlServer(connectionString);

        return new DBContext(optionsBuilder.Options, null);
    }
}
