
using Microsoft.Extensions.Logging;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Shared.Dtos.Payroll;

namespace ESSPortal.Application.Contracts.Implementations.Services;


internal sealed class PdfGenerationService : IPdfGenerationService
{
    private readonly ILogger<PdfGenerationService> _logger;

    public PdfGenerationService(ILogger<PdfGenerationService> logger)
    {
        _logger = logger;
    }

    
    public Task<byte[]> GeneratePayslipPdfAsync(string base64Data, PrintPaySlipRequest request)
    {
        try
        {
            return Task.FromResult(ConvertBase64ToPdfBytes(base64Data));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payslip PDF");
            throw;
        }
    }

    public Task<byte[]> GenerateP9PdfAsync(string base64Data, PrintP9Request request)
    {
        try
        {
            return Task.FromResult(ConvertBase64ToPdfBytes(base64Data));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating P9 PDF");
            throw;
        }
    }

    private byte[] ConvertBase64ToPdfBytes(string base64Data)
    {
        try
        {
            var pdfBytes = Convert.FromBase64String(base64Data);
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting base64 to PDF bytes");
            throw;
        }
    }


}
