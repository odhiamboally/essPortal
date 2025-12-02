namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Common;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    T? Get<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan absoluteExpiration, TimeSpan slidingExpiration);
    void Set<T>(string key, T value, TimeSpan expiration);


    Task RemoveAsync(string key);
    void Remove(string key);
    void RemoveByPattern(string pattern);

    bool Exists(string key);
}
