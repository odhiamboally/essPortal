using ESSPortal.Application.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface INavisionService
{
    Task<ApiResponse<(T, string RawJson)>> CreateAsync<T>(string serviceName, T entity) where T : class, new();

    Task<ApiResponse<(T, string RawJson)>> CreateLeaveApplicationAsync<T>(string serviceName, T entity) where T : class, new();
    Task<ApiResponse<(T, string RawJson)>> UpdateLeaveApplicationAsync<T>(string serviceName, T entity) where T : class, new();

    Task<ApiResponse<List<T>>> CreateMultipleAsync<T>(string serviceName, List<T> entities) where T : class, new();

    Task<ApiResponse<T>> GetSingleAsync<T>(string requestUri) where T : class, new();
    Task<ApiResponse<(List<T>, string RawJson)>> GetMultipleAsync<T>(string requestUri) where T : class, new();
    Task<ApiResponse<(List<T>, string RawJson)>> GetMultipleAsyncSimple<T>(string requestUri) where T : class, new();

    Task<ApiResponse<string>> GenerateP9Async(string employeeNo, int year);
    Task<ApiResponse<string>> GeneratePaySlipAsync(string employeeNo, DateTime period);

    Task<ApiResponse<bool>> PostAsync(string serviceName);
    Task<ApiResponse<bool>> PostAsync(string serviceName, Dictionary<string, string> parameters);
    Task<ApiResponse<bool>> PostAsync<T>(string requestUri, T filter) where T : class, new();


}
