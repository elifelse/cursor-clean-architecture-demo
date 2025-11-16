using FluentValidation;
using System.Reflection;
using CursorDemo.Application.Validators;

namespace CursorDemo.Api.Extensions;

/// <summary>
/// Extension methods for service collection configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers FluentValidation validators from the Application layer
    /// </summary>
    public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        // Register validators from the Application assembly
        services.AddValidatorsFromAssemblyContaining<CreateBookDtoValidator>();
        
        return services;
    }
}

