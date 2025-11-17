using Asp.Versioning;
using CursorDemo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursorDemo.Api.Controllers;

/// <summary>
/// Cache testing and debugging endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiVersionNeutral]
[Authorize]
public class CacheTestController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheTestController> _logger;

    public CacheTestController(ICacheService cacheService, ILogger<CacheTestController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Test cache by setting and getting a value
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expirationSeconds">Expiration time in seconds (default: 30)</param>
    [HttpPost("test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult TestCache([FromQuery] string key = "test:key", [FromQuery] string value = "test-value", [FromQuery] int expirationSeconds = 30)
    {
        // Set value in cache
        _cacheService.Set(key, new { Message = value, Timestamp = DateTime.UtcNow }, TimeSpan.FromSeconds(expirationSeconds));
        _logger.LogInformation("Set cache key: {Key} with value: {Value}, expires in {Seconds} seconds", key, value, expirationSeconds);

        // Get value from cache
        var cachedValue = _cacheService.Get<object>(key);
        
        if (cachedValue != null)
        {
            return Ok(new
            {
                success = true,
                message = "Cache test successful",
                key = key,
                cachedValue = cachedValue,
                expirationSeconds = expirationSeconds
            });
        }

        return BadRequest(new
        {
            success = false,
            message = "Failed to retrieve cached value"
        });
    }

    /// <summary>
    /// Get a cached value by key
    /// </summary>
    /// <param name="key">Cache key</param>
    [HttpGet("get/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetCacheValue(string key)
    {
        var value = _cacheService.Get<object>(key);
        
        if (value == null)
        {
            return NotFound(new
            {
                success = false,
                message = $"Cache key '{key}' not found or expired",
                key = key
            });
        }

        return Ok(new
        {
            success = true,
            key = key,
            value = value
        });
    }

    /// <summary>
    /// Remove a cache entry by key
    /// </summary>
    /// <param name="key">Cache key</param>
    [HttpDelete("remove/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult RemoveCacheValue(string key)
    {
        _cacheService.Remove(key);
        _logger.LogInformation("Removed cache key: {Key}", key);
        
        return Ok(new
        {
            success = true,
            message = $"Cache key '{key}' removed",
            key = key
        });
    }

    /// <summary>
    /// Remove cache entries by pattern (e.g., "books:*")
    /// </summary>
    /// <param name="pattern">Cache key pattern</param>
    [HttpDelete("remove-pattern")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult RemoveCacheByPattern([FromQuery] string pattern = "books:*")
    {
        _cacheService.RemoveByPattern(pattern);
        _logger.LogInformation("Removed cache entries matching pattern: {Pattern}", pattern);
        
        return Ok(new
        {
            success = true,
            message = $"Cache entries matching pattern '{pattern}' removed",
            pattern = pattern
        });
    }
}

