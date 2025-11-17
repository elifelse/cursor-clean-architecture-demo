using CursorDemo.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace CursorDemo.Infrastructure.Services;

/// <summary>
/// In-memory cache implementation using IMemoryCache
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, object> _keyRegistry = new();
    private readonly object _lockObject = new object();

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public T? Get<T>(string key) where T : class
    {
        return _memoryCache.Get<T>(key);
    }

    public void Set<T>(string key, T value, TimeSpan expiration) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            SlidingExpiration = null
        };

        // Register key for pattern-based removal
        _keyRegistry.TryAdd(key, null!);

        // Remove key from registry when cache entry expires
        options.RegisterPostEvictionCallback((key, value, reason, state) =>
        {
            _keyRegistry.TryRemove(key.ToString()!, out _);
        });

        _memoryCache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
        _keyRegistry.TryRemove(key, out _);
    }

    public void RemoveByPattern(string pattern)
    {
        // Convert pattern to regex (e.g., "books:*" becomes "^books:.*$")
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

        lock (_lockObject)
        {
            var keysToRemove = _keyRegistry.Keys
                .Where(key => regex.IsMatch(key))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _keyRegistry.TryRemove(key, out _);
            }
        }
    }
}

