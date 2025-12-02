using EssPortal.Application.Dtos.ModelFilters;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IEmployeeService
{
    Task<ApiResponse<PagedResult<Employees>>> GetEmployeesAsync();
    Task<ApiResponse<Employees>> GetEmployeeByNoAsync(string employeeNo);
    Task<ApiResponse<PagedResult<Employees>>> SearchEmployeesAsync(EmployeesFilter filter);

    Task<ApiResponse<PagedResult<EmployeeCard>>> GetEmployeeCardsAsync();
    Task<ApiResponse<EmployeeCard>> GetEmployeeCardByNoAsync(string employeeNo);
    
    Task<ApiResponse<PagedResult<EmployeeCard>>> SearchEmployeeCardsAsync(EmployeeCardFilter filter);

    

}
