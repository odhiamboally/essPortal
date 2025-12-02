namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface INavisionUrlHelper
{
    string GetODataUrl(string entitySetKey);
    string GetEssUrl(string serviceName);
    bool IsEssService(string entitySetKey);
}
