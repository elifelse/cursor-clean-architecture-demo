using CursorDemo.Api.Middleware;

namespace CursorDemo.Api.Extensions;

/// <summary>
/// Extension methods for application builder configuration
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds request logging middleware to the pipeline
    /// Logs HTTP method, path, status code, and response time
    /// </summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }

    /// <summary>
    /// Adds global exception handling middleware to the pipeline
    /// This middleware catches all unhandled exceptions and returns standardized error responses
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}

