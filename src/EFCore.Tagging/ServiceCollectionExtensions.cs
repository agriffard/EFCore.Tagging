using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Tagging;

/// <summary>
/// Extension methods for configuring EF Core Tagging services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds EF Core Tagging services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddEfCoreTagging(
        this IServiceCollection services,
        Action<EfTaggingOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<EfTaggingOptions>(_ => { });
        }

        return services;
    }
}

/// <summary>
/// Extension methods for configuring the EF Core Tagging middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the EF Core Tagging middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for method chaining.</returns>
    public static IApplicationBuilder UseEfCoreTagging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TaggingMiddleware>();
    }
}
