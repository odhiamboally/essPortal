using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Employee;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IEmployeeService
{
    Task<ApiResponse<PagedResult<EmployeeResponse>>> GetEmployeesAsync();
    Task<ApiResponse<EmployeeResponse>> GetEmployeeByNoAsync(string employeeNo);
    Task<ApiResponse<PagedResult<EmployeeResponse>>> SearchEmployeesAsync(EmployeesFilter filter);

    Task<ApiResponse<bool>> CreateEmployeeCardsAsync(CreateEmployeeCardRequest request);
    Task<ApiResponse<PagedResult<EmployeeCardResponse>>> GetEmployeeCardsAsync();
    Task<ApiResponse<EmployeeCardResponse>> GetEmployeeCardByNoAsync(string employeeNo);

    Task<ApiResponse<PagedResult<EmployeeCardResponse>>> SearchEmployeeCardsAsync(EmployeeCardFilter filter);


}
