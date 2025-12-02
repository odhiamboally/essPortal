using EssPortal.Web.Mvc.Dtos.Common;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface IApiService
{
    Task<AppResponse<TResponse?>> GetAsync<TResponse>(string endpoint);
    Task<AppResponse<PagedResult<TResponse>>> GetPagedAsync<TResponse>(string endpoint);
    Task<AppResponse<PagedResult<TResponse>>> GetPagedAsync<TRequest, TResponse>(string endpoint, TRequest? request);
    Task<AppResponse<TResponse?>> GetAsync<TRequest, TResponse>(string endpoint, TRequest? request);
    Task<AppResponse<TResponse?>> PostAsync<TRequest, TResponse>(string endpoint, TRequest? request);
    Task<AppResponse<TResponse?>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request);
    Task<AppResponse<TResponse?>> DeleteAsync<TResponse>(string endpoint);
    Task<AppResponse<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest request);
}
