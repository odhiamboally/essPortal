using ESSPortal.Shared.Contracts.Implementations.Common;
using ESSPortal.Shared.Contracts.Interfaces.Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace EssPortal.Shared.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {


        return services;
    }
}
