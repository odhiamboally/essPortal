using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;

using Microsoft.Extensions.Caching.Memory;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.Common;

internal sealed class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<InMemoryCacheService> _logger;

    public InMemoryCacheService(IMemoryCache memoryCache, ILogger<InMemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        try
        {
            return _memoryCache.TryGetValue(key, out T? value) ? value : default!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached data for key {Key}", key);
            return default;
        }
        
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        return await Task.FromResult(Get<T>(key));
    }

    public void Remove(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _logger.LogDebug("Removed cache for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached data for key {Key}", key);
            throw;
        }
        
    }

    public async Task RemoveAsync(string key)
    {
        Remove(key);
        await Task.CompletedTask;
    }

    public void Set<T>(string key, T value, TimeSpan expiration)
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.Add(expiration),
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = TimeSpan.FromMinutes(Math.Min(expiration.TotalMinutes / 3, 15)), // 1/3 of absolute or max 15 min
                Priority = CacheItemPriority.Normal,
                Size = 1 // Add this line - each entry counts as 1 unit
            };
            _memoryCache.Set(key, value, options);
            _logger.LogDebug("Cached data for key {Key} with expiration {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached data for key {Key}", key);
            throw;
        }
        
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
        return Task.CompletedTask;
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
