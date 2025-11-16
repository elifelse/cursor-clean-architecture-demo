using CursorDemo.Api.Middleware;

namespace CursorDemo.Api.Extensions;

/// <summary>
/// Extension methods for application builder configuration
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds global exception handling middleware to the pipeline
    /// This middleware catches all unhandled exceptions and returns standardized error responses
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}

