using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Payroll;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using ESSPortal.Web.Blazor.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.AppServices;

internal sealed class PayrollService : IPayrollService
{
    private readonly ILogger<PayrollService> _logger;
    private readonly IServiceManager _serviceManager;


    public PayrollService(ILogger<PayrollService> logger, IServiceManager serviceManager)
    {
        _logger = logger;
        _serviceManager = serviceManager;
    }

    public async Task<ApiResponse<byte[]>> GenerateP9Async(PrintP9Request request)
    {
        try
        {
            var apiResponse = await _serviceManager.PayrollService.GenerateP9Async(request);

            return apiResponse.Successful
                    ? ApiResponse<byte[]>.Success(apiResponse.Message!, apiResponse.Data!)
                    : ApiResponse<byte[]>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating P9 for EmployeeNo: {EmployeeNo}, Year: {Year}", request.EmployeeNo, request.Year);

            throw;
        }
        

    }

    public async Task<ApiResponse<byte[]>> GeneratePayslipAsync(PrintPaySlipRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.PayrollService.GeneratePayslipAsync(request);

            return apiResponse.Successful
                    ? ApiResponse<byte[]>.Success(apiResponse.Message!, apiResponse.Data!)
                    : ApiResponse<byte[]>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Payslip for EmployeeNo: {EmployeeNo}, Month: {Month}, Year: {Year}", request.EmployeeNo, request.Month, request.Year);
            throw;
        }

    }

    
}
