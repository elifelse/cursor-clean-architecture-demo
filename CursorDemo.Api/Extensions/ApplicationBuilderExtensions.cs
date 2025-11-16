using CursorDemo.Api.Middleware;

namespace CursorDemo.Api.Extensions;

/// <summary>
/// Extension methods for application builder configuration
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds global exception handling middleware to the pipeline
    /// This should be called early in the pipeline, before other middleware that writes responses
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

