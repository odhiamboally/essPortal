


using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Payroll;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IPayrollService
{
    Task<AppResponse<byte[]>> GeneratePayslipAsync(PrintPaySlipRequest request);
    Task<AppResponse<byte[]>> GenerateP9Async(PrintP9Request request);
}
