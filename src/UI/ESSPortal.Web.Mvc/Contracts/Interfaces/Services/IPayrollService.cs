using EssPortal.Web.Mvc.Dtos.Common;
using ESSPortal.Web.Mvc.Dtos.Payroll;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface IPayrollService
{
    Task<AppResponse<byte[]>> GeneratePayslipAsync(PrintPaySlipRequest request);
    Task<AppResponse<byte[]>> GenerateP9Async(PrintP9Request request);
    
}
