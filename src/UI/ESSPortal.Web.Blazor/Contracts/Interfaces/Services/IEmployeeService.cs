using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Dtos.Common;
using EssPortal.Web.Blazor.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Models.Navision;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Employee;


namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface IEmployeeService
{
    // Employees
    Task<ApiResponse<PagedResult<EmployeeResponse>>> GetEmployeesAsync();
    Task<ApiResponse<EmployeeResponse>> GetEmployeeByNoAsync(string employeeNo);
    Task<ApiResponse<PagedResult<EmployeeResponse>>> SearchEmployeesAsync(EmployeesFilter filter);

    // Employee Cards
    Task<ApiResponse<bool>> CreateEmployeeCardAsync(CreateEmployeeCardRequest request);
    Task<ApiResponse<PagedResult<EmployeeCardResponse>>> GetEmployeeCardsAsync();
    Task<ApiResponse<EmployeeCardResponse>> GetEmployeeCardByNoAsync(string employeeNo);
    Task<ApiResponse<PagedResult<EmployeeCardResponse>>> SearchEmployeeCardsAsync(EmployeeCardFilter filter);


}
