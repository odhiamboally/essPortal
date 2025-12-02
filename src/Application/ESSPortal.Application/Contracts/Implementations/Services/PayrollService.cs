

using ESSPortal.Application.Configuration;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Payroll;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESSPortal.Application.Contracts.Implementations.Services;
internal sealed class PayrollService : IPayrollService
{
    private readonly INavisionService _navisionService;
    private readonly IPdfGenerationService _pdfGenerationService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<PayrollService> _logger;
    private readonly BCSettings _bcSettings;

    public PayrollService(
        ICacheService cacheService,
        IPdfGenerationService pdfGenerationService,
        ILogger<PayrollService> logger, 
        IOptions<BCSettings> bcSettings,
        INavisionService navisionService)
    {
        _cacheService = cacheService;
        _logger = logger;
        _bcSettings = bcSettings.Value;
        _navisionService = navisionService;
        _pdfGenerationService = pdfGenerationService;
    }


    public async Task<ApiResponse<byte[]>> GenerateP9Async(PrintP9Request request)
    {
        try
        {
            var base64Response = await _navisionService.GenerateP9Async(request.EmployeeNo, request.Year);

            if (!base64Response.Successful || string.IsNullOrWhiteSpace(base64Response.Data))
            {
                _logger.LogWarning("Failed to retrieve P9 data from BC for employee {EmployeeNo}", request.EmployeeNo);

                return ApiResponse<byte[]>.Failure("Unable to retrieve P9 data from Business Central.");
            }

            var pdfBytes = await _pdfGenerationService.GenerateP9PdfAsync(base64Response.Data, request);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                return ApiResponse<byte[]>.Failure("Failed to generate P9 PDF.");
            }

            return base64Response.Successful
                ? ApiResponse<byte[]>.Success("P9 retrieved", pdfBytes)
                : ApiResponse<byte[]>.Failure(base64Response.Message ?? "Failed to retrieve P9.");
        }
        catch (Exception)
        {

            throw;
        }
        
    }

    public async Task<ApiResponse<byte[]>> GeneratePayslipAsync(PrintPaySlipRequest request)
    {
        try
        {
            var periodDate = new DateTime(request.Year, request.Month, 1);

            var base64Response = await _navisionService.GeneratePaySlipAsync(request.EmployeeNo, periodDate);

            if (!base64Response.Successful || string.IsNullOrWhiteSpace(base64Response.Data))
            {
                _logger.LogWarning("Failed to retrieve payslip data from BC for employee {EmployeeNo}", request.EmployeeNo);
                return ApiResponse<byte[]>.Failure(base64Response.Message ?? "Unable to retrieve payslip data from Business Central.");
            }

            var pdfBytes = await _pdfGenerationService.GeneratePayslipPdfAsync(base64Response.Data, request);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                return ApiResponse<byte[]>.Failure("Failed to generate payslip PDF.");
            }

            _logger.LogInformation("Successfully generated payslip for employee {EmployeeNo}", request.EmployeeNo);

            return base64Response.Successful
                ? ApiResponse<byte[]>.Success("Payslip retrieved", pdfBytes)
                : ApiResponse<byte[]>.Failure(base64Response.Message ?? "Failed to retrieve payslip.");
        }
        catch (Exception)
        {

            throw;
        }
    }

}
