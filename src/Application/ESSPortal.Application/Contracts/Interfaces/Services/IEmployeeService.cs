using EssPortal.Shared.Dtos.Employee;
using EssPortal.Shared.Dtos.ModelFilters;


using ESSPortal.Domain.NavEntities;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Employee;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IEmployeeService
{
    Task<AppResponse<PagedResult<EmployeeResponse>>> GetEmployeesAsync();
    Task<AppResponse<EmployeeResponse>> GetEmployeeByNoAsync(string employeeNo);
    Task<AppResponse<PagedResult<EmployeeResponse>>> SearchEmployeesAsync(EmployeesFilter filter);

    Task<AppResponse<bool>> CreateEmployeeCardsAsync(CreateEmployeeCardRequest request);
    Task<AppResponse<PagedResult<EmployeeCardResponse>>> GetEmployeeCardsAsync();
    Task<AppResponse<EmployeeCardResponse>> GetEmployeeCardByNoAsync(string employeeNo);

    Task<AppResponse<PagedResult<EmployeeCardResponse>>> SearchEmployeeCardsAsync(EmployeeCardFilter filter);


}
