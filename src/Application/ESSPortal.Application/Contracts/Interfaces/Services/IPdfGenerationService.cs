using ESSPortal.Application.Dtos.Payroll;

namespace ESSPortal.Application.Contracts.Interfaces.Services;


internal interface IPdfGenerationService
{
    Task<byte[]> GeneratePayslipPdfAsync(string base64Data, PrintPaySlipRequest request);
    Task<byte[]> GenerateP9PdfAsync(string base64Data, PrintP9Request request);
    
}
