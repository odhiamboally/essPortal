

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Payroll;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IPayrollService
{
    Task<ApiResponse<byte[]>> GeneratePayslipAsync(PrintPaySlipRequest request);
    Task<ApiResponse<byte[]>> GenerateP9Async(PrintP9Request request);
}
