


using ESSPortal.Application.Configuration;

using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace ESSPortal.Application.Utilities;

public class BasicAuthHandler : DelegatingHandler
{
    private readonly IOptions<BCSettings> _settings;
    public BasicAuthHandler(IOptions<BCSettings> settings)
    {
        _settings = settings;

    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var settings = _settings.Value;
            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}"));

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                
            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception)
        {

            throw;
        }
       
    }
}
