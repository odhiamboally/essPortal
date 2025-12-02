using ESSPortal.Application.Contracts.Interfaces.Common;

using Microsoft.Extensions.Caching.Memory;

namespace ESSPortal.Application.Contracts.Implementations.Caching;
internal class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public T? Get<T>(string key)
    {
        return _memoryCache.TryGetValue(key, out T? value) ? value : default!;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        return await Task.FromResult(Get<T>(key));
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public Task RemoveAsync(string key)
    {
        Remove(key); 
        return Task.CompletedTask;
    }

    public void Set<T>(string key, T value, TimeSpan expiration)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            SlidingExpiration = TimeSpan.FromMinutes(Math.Min(expiration.TotalMinutes / 3, 15)), // 1/3 of absolute or max 15 min
            Priority = CacheItemPriority.Normal,
            Size = 1 // Add this line - each entry counts as 1 unit
        };
        _memoryCache.Set(key, value, options);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan absoluteExpiration, TimeSpan slidingExpiration)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration,
            SlidingExpiration = slidingExpiration,
            Priority = CacheItemPriority.Normal
        };

        _memoryCache.Set(key, value, options);

        return Task.CompletedTask; // No async, no Task.Run
    }

    public void Set(string cacheKey, DateTimeOffset utcNow, MemoryCacheEntryOptions options)
    {
        _memoryCache.Set(cacheKey, utcNow, options);
    }

    public bool Exists(string key)
    {
        return _memoryCache.TryGetValue(key, out _);
    }

    public void RemoveByPattern(string pattern)
    {
        throw new NotImplementedException();
    }

    
}
