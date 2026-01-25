using ESSPortal.Shared.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface INavisionService
{
    Task<AppResponse<(T, string RawJson)>> CreateAsync<T>(string serviceName, T entity) where T : class, new();

    Task<AppResponse<(T, string RawJson)>> CreateLeaveApplicationAsync<T>(string serviceName, T entity) where T : class, new();
    Task<AppResponse<(T, string RawJson)>> UpdateLeaveApplicationAsync<T>(string serviceName, T entity) where T : class, new();

    Task<AppResponse<List<T>>> CreateMultipleAsync<T>(string serviceName, List<T> entities) where T : class, new();

    Task<AppResponse<T>> GetSingleAsync<T>(string requestUri) where T : class, new();
    Task<AppResponse<(List<T>, string RawJson)>> GetMultipleAsync<T>(string requestUri) where T : class, new();
    Task<AppResponse<(List<T>, string RawJson)>> GetMultipleAsyncSimple<T>(string requestUri) where T : class, new();

    Task<AppResponse<string>> GenerateP9Async(string employeeNo, int year);
    Task<AppResponse<string>> GeneratePaySlipAsync(string employeeNo, DateTime period);

    Task<AppResponse<bool>> PostAsync(string serviceName);
    Task<AppResponse<bool>> PostAsync(string serviceName, Dictionary<string, string> parameters);
    Task<AppResponse<bool>> PostAsync<T>(string requestUri, T filter) where T : class, new();


}
