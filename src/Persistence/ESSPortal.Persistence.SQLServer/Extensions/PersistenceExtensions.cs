using Microsoft.EntityFrameworkCore.Infrastructure;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ESSPortal.Persistence.SQLServer.Extensions;

public static class PersistenceExtensions
{
    public static void ConfigureSqlOptions(SqlServerDbContextOptionsBuilder sqlOptions)
    {
        // NOTE:
        // Manual transactions are used across the app.
        // SqlServer retry strategy is intentionally disabled.
        // Future refactor should migrate to execution-strategy-based transactions.

        //sqlOptions.EnableRetryOnFailure(
        //    maxRetryCount: 5,
        //    maxRetryDelay: TimeSpan.FromSeconds(30),
        //    errorNumbersToAdd: null);

        sqlOptions.CommandTimeout(30);
        //sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
    }
}
