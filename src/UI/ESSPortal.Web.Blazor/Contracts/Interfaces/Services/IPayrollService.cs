using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Payroll;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface IPayrollService
{
    Task<ApiResponse<byte[]>> GeneratePayslipAsync(PrintPaySlipRequest request);
    Task<ApiResponse<byte[]>> GenerateP9Async(PrintP9Request request);
    
}
