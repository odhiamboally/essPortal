using EssPortal.Shared.Dtos.Employee;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Employee;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface IEmployeeService
{
    // Employees
    Task<AppResponse<List<EmployeeResponse>>> GetEmployeesAsync();
    Task<AppResponse<EmployeeResponse>> GetEmployeeByNoAsync(string employeeNo);
    Task<AppResponse<EmployeeResponse>> GetEmployeeByRecIdAsync(string recId);
    Task<AppResponse<List<EmployeeResponse>>> SearchEmployeesAsync(EmployeesFilter filter);

    // Employee Cards
    Task<AppResponse<List<EmployeeCardResponse>>> GetEmployeeCardsAsync();
    Task<AppResponse<EmployeeCardResponse>> GetEmployeeCardByNoAsync(string employeeNo);
    Task<AppResponse<EmployeeCardResponse>> GetEmployeeCardByRecIdAsync(string recId);
    Task<AppResponse<bool>> CreateEmployeeCardAsync(CreateEmployeeCardRequest request);
    Task<AppResponse<bool>> UpdateEmployeeCardAsync(CreateEmployeeCardRequest request);
    Task<AppResponse<bool>> DeleteEmployeeCardAsync(string key);
    Task<AppResponse<List<EmployeeCardResponse>>> SearchEmployeeCardsAsync(EmployeeCardFilter filter);

    // Utilities
    Task<AppResponse<string>> GetRecIdFromKeyAsync(string key);
    Task<AppResponse<EmployeeCardResponse>> GetUserEmailAsync(string? odataQuery = null);
    Task<AppResponse<EmployeeCardResponse>> CheckEmployeeNumberAsync(string? odataQuery = null);

}
