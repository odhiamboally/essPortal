using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Services;

using Microsoft.Extensions.Options;

namespace ESSPortal.Infrastructure.Implementations.Services;
internal sealed class NavisionUrlHelper : INavisionUrlHelper
{
    private readonly BCSettings _bCettings;

    private readonly HashSet<string> _essServices =
    [
        "EmployeeSelfService_GetLeaveApplications",
        "EmployeeSelfService_UpdateLeaveApplication"

    ];

    public NavisionUrlHelper(IOptions<BCSettings> bCettings)
    {
        _bCettings = bCettings.Value;
    }

    public string GetODataUrl(string entitySetKey)
    {
        if (_bCettings.EntitySets.TryGetValue(entitySetKey, out var entitySet))
        {
            return $"{_bCettings.OdataBaseUrl.TrimEnd('/')}/{entitySet}";
        }
        throw new ArgumentException($"EntitySet '{entitySetKey}' not found in configuration", nameof(entitySetKey));
    }

    public string GetEssUrl(string serviceName)
    {
        return _bCettings.EmployeeSelfServiceEndpoint.Replace("{serviceName}", Uri.EscapeDataString(serviceName)).TrimEnd('/');
    }

    public bool IsEssService(string entitySetKey)
    {
        return _essServices.Contains(entitySetKey);
    }
}
