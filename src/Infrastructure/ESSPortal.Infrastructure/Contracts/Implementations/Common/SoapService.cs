using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Infrastructure.SoapServices;
using ESSPortal.Shared.Dtos.Common;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.ServiceModel;
using System.Xml;

namespace ESSPortal.Infrastructure.Contracts.Implementations.Common;
internal sealed class SoapService : ISoapService
{
    private readonly BCSettings _bcSettings;
    private readonly ILogger<SoapService> _logger;

    public SoapService(IOptions<BCSettings> bcSettings, ILogger<SoapService> logger)
    {
        _bcSettings = bcSettings.Value;
        _logger = logger;
    }

    public async Task<AppResponse<string>> GenerateP9Async(string employeeNo, int year)
    {
        try
        {
            using var soapClient = CreateSoapClient();

            var request = new PrintP9
            {
                employeeNo = employeeNo,
                path = Path.GetTempPath(),
                year = year,
                p9Base64Txt = string.Empty
            };

            var response = await soapClient.PrintP9Async(request);

            if (string.IsNullOrWhiteSpace(response.p9Base64Txt))
            {
                return AppResponse<string>.Failure("P9 generation failed - no data returned");
            }

            return AppResponse<string>.Success("P9 generated successfully", response.p9Base64Txt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating P9 for employee {EmployeeNo}", employeeNo);
            throw;
        }
    }

    public async Task<AppResponse<string>> GeneratePaySlipAsync(string employeeNo, DateTime period)
    {
        try
        {
            using var soapClient = CreateSoapClient();

            var request = new PrintPaySlip
            {
                employeeNo = employeeNo,
                path = Path.GetTempPath(),
                period = period,
                payslipTxt = string.Empty
            };

            var response = await soapClient.PrintPaySlipAsync(request);

            if (string.IsNullOrWhiteSpace(response.payslipTxt))
            {
                return AppResponse<string>.Failure("PaySlip generation failed - no data returned");
            }

            return AppResponse<string>.Success("PaySlip generated successfully", response.payslipTxt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PaySlip for employee {EmployeeNo}", employeeNo);
            throw;
        }
    }



    private OnlinePortalServices_PortClient CreateSoapClient()
    {
        var client = new OnlinePortalServices_PortClient();

        // Configure Basic Authentication
        if (!string.IsNullOrWhiteSpace(_bcSettings.Username) && !string.IsNullOrWhiteSpace(_bcSettings.Password))
        {
            client.ClientCredentials.UserName.UserName = _bcSettings.Username;
            client.ClientCredentials.UserName.Password = _bcSettings.Password;
        }

        // Configure the binding for Basic Authentication
        if (client.Endpoint.Binding is BasicHttpBinding basicBinding)
        {
            // Enable Basic Authentication
            basicBinding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
            basicBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

            // Set timeouts
            basicBinding.SendTimeout = TimeSpan.FromSeconds(_bcSettings.TimeoutSeconds);
            basicBinding.ReceiveTimeout = TimeSpan.FromSeconds(_bcSettings.TimeoutSeconds);

            // Increase message size limits if needed
            basicBinding.MaxBufferSize = int.MaxValue;
            basicBinding.MaxReceivedMessageSize = int.MaxValue;
            basicBinding.ReaderQuotas = XmlDictionaryReaderQuotas.Max;
        }

        // Override endpoint address if needed
        if (!string.IsNullOrWhiteSpace(_bcSettings.OnlinePortalServicesEndpoint))
        {
            client.Endpoint.Address = new EndpointAddress(_bcSettings.OnlinePortalServicesEndpoint);
        }

        return client;
    }

}
