using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Dtos.Payroll;
using ESSPortal.Web.Mvc.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.AppServices;

internal sealed class PayrollService : IPayrollService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;


    public PayrollService(IApiService apiService, IOptions<ApiSettings> apiSettings)
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;

    }

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

            var apiResponse = await _apiService.PostAsync<PrintP9Request, byte[]>(endpoint, request);

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

            var apiResponse = await _apiService.PostAsync<PrintPaySlipRequest, byte[]>(endpoint, request);
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
