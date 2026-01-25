using EssPortal.Shared.Configurations;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Payroll;
using ESSPortal.Shared.Utilities.Api;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

using Microsoft.Extensions.Options;

namespace ESSPortal.Shared.Contracts.Implementations.Services;

internal sealed class PayrollService(
    IServiceManager serviceManager,
    IApiService apiService, 
    IOptions<ApiSettings> apiSettings

 ) : IPayrollService
{
    private readonly ApiSettings _apiSettings = apiSettings.Value;

    public async Task<AppResponse<byte[]>> GenerateP9Async(PrintP9Request request)
    {
        try
        {


            var endpoint = _apiSettings.ApiEndpoints.Payroll.GenerateP9;
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return AppResponse<byte[]>.Failure("Endpoint is not configured.");
            }

            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await apiService.PostAsync<PrintP9Request, byte[]>(endpoint, request);

            return apiResponse.Successful
                    ? AppResponse<byte[]>.Success(apiResponse.Message!, apiResponse.Data!)
                    : AppResponse<byte[]>.Failure(apiResponse.Message!);
        }
        catch (Exception)
        {

            throw;
        }
        

    }

    public async Task<AppResponse<byte[]>> GeneratePayslipAsync(PrintPaySlipRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.Payroll.GeneratePayslip;
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return AppResponse<byte[]>.Failure("Endpoint is not configured.");
            }

            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);

            var apiResponse = await apiService.PostAsync<PrintPaySlipRequest, byte[]>(endpoint, request);
            return apiResponse.Successful
                    ? AppResponse<byte[]>.Success(apiResponse.Message!, apiResponse.Data!)
                    : AppResponse<byte[]>.Failure(apiResponse.Message!);
        }
        catch (Exception)
        {
            throw;
        }

    }

    
}
