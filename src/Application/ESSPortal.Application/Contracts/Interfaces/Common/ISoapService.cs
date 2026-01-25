
using ESSPortal.Shared.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Common;

public interface ISoapService
{
    Task<AppResponse<string>> GenerateP9Async(string employeeNo, int year);
    Task<AppResponse<string>> GeneratePaySlipAsync(string employeeNo, DateTime period);
}
