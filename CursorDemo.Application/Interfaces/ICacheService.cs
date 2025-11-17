namespace CursorDemo.Application.Interfaces;

/// <summary>
/// Interface for caching operations
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache
    /// </summary>
    /// <typeparam name="T">Type of the cached value</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached value if found, otherwise null</returns>
    T? Get<T>(string key) where T : class;

    /// <summary>
    /// Sets a value in cache with expiration
    /// </summary>
    /// <typeparam name="T">Type of the value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expiration">Time until expiration</param>
    void Set<T>(string key, T value, TimeSpan expiration) where T : class;

    /// <summary>
    /// Removes a value from cache
    /// </summary>
    /// <param name="key">Cache key</param>
    void Remove(string key);

    /// <summary>
    /// Removes all cache entries that match the key pattern
    /// </summary>
    /// <param name="pattern">Key pattern (e.g., "books:*")</param>
    void RemoveByPattern(string pattern);
}

