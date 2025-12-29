using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace EFCore.Tagging;

/// <summary>
/// Middleware that automatically creates a tag scope for each HTTP request.
/// </summary>
public class TaggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly EfTaggingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">The tagging options.</param>
    public TaggingMiddleware(RequestDelegate next, IOptions<EfTaggingOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        var controllerName = GetControllerName(endpoint);
        var actionName = GetActionName(endpoint);

        using var scope = TagScope.Begin(
            !string.IsNullOrEmpty(controllerName) ? controllerName : "Request",
            actionName);

        // Add HTTP method
        scope.WithMetadata("Method", context.Request.Method);

        // Add endpoint information
        if (_options.IncludeEndpoint)
        {
            scope.WithMetadata("Path", context.Request.Path.Value ?? "/");
        }

        // Add user information
        if (_options.IncludeUser && context.User.Identity?.IsAuthenticated == true)
        {
            var userName = context.User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                scope.WithMetadata("User", userName);
            }
        }

        // Add correlation ID
        if (_options.IncludeCorrelationId)
        {
            var correlationId = GetOrCreateCorrelationId(context);
            scope.WithMetadata("CorrelationId", correlationId);
        }

        await _next(context);
    }

    private static string? GetControllerName(Endpoint? endpoint)
    {
        if (endpoint == null) return null;

        var controllerActionDescriptor = endpoint.Metadata
            .GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();

        return controllerActionDescriptor?.ControllerName;
    }

    private static string? GetActionName(Endpoint? endpoint)
    {
        if (endpoint == null) return null;

        var controllerActionDescriptor = endpoint.Metadata
            .GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();

        return controllerActionDescriptor?.ActionName;
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(_options.CorrelationIdHeader, out var correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        var newCorrelationId = Guid.NewGuid().ToString("N")[..8];
        context.Response.Headers[_options.CorrelationIdHeader] = newCorrelationId;
        return newCorrelationId;
    }
}
