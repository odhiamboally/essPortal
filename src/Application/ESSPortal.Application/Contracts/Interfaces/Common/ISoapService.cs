using ESSPortal.Application.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Common;

public interface ISoapService
{
    Task<ApiResponse<string>> GenerateP9Async(string employeeNo, int year);
    Task<ApiResponse<string>> GeneratePaySlipAsync(string employeeNo, DateTime period);
}
