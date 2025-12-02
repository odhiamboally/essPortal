using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

using ESSPortal.Web.Mvc.Dtos.Employee;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface IEmployeeService
{
    // Employees
    Task<AppResponse<List<Employees>>> GetEmployeesAsync();
    Task<AppResponse<Employees>> GetEmployeeByNoAsync(string employeeNo);
    Task<AppResponse<Employees>> GetEmployeeByRecIdAsync(string recId);
    Task<AppResponse<List<Employees>>> SearchEmployeesAsync(EmployeesFilter filter);

    // Employee Cards
    Task<AppResponse<List<EmployeeCard>>> GetEmployeeCardsAsync();
    Task<AppResponse<EmployeeCard>> GetEmployeeCardByNoAsync(string employeeNo);
    Task<AppResponse<EmployeeCard>> GetEmployeeCardByRecIdAsync(string recId);
    Task<AppResponse<bool>> CreateEmployeeCardAsync(CreateEmployeeCardRequest request);
    Task<AppResponse<bool>> UpdateEmployeeCardAsync(CreateEmployeeCardRequest request);
    Task<AppResponse<bool>> DeleteEmployeeCardAsync(string key);
    Task<AppResponse<List<EmployeeCard>>> SearchEmployeeCardsAsync(EmployeeCardFilter filter);

    // Utilities
    Task<AppResponse<string>> GetRecIdFromKeyAsync(string key);
    Task<AppResponse<EmployeeCard>> GetUserEmailAsync(string? odataQuery = null);
    Task<AppResponse<EmployeeCard>> CheckEmployeeNumberAsync(string? odataQuery = null);

}
