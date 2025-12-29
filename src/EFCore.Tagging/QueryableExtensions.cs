using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Tagging;

/// <summary>
/// Extension methods for IQueryable to add tagging support.
/// </summary>
public static class QueryableExtensions
{
    // Cache for property info to avoid repeated reflection
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

    /// <summary>
    /// Adds a contextual tag to the query with the specified name and metadata.
    /// </summary>
    /// <typeparam name="T">The type of the query elements.</typeparam>
    /// <param name="query">The query to tag.</param>
    /// <param name="name">The name of the tag.</param>
    /// <param name="metadata">Optional anonymous object containing metadata key-value pairs.</param>
    /// <returns>The tagged query.</returns>
    public static IQueryable<T> TagWithContext<T>(this IQueryable<T> query, string name, object? metadata = null)
    {
        var tag = new QueryTag { Name = name };

        if (metadata != null)
        {
            var metadataType = metadata.GetType();
            var properties = _propertyCache.GetOrAdd(metadataType, t => t.GetProperties());
            
            foreach (var property in properties)
            {
                var value = property.GetValue(metadata);
                if (value != null)
                {
                    tag.Metadata[property.Name] = value.ToString() ?? string.Empty;
                }
            }
        }

        // Add tag from scope if available
        var scopeTag = TagScope.Current;
        if (scopeTag != null)
        {
            foreach (var kvp in scopeTag.Tag.Metadata)
            {
                if (!tag.Metadata.ContainsKey(kvp.Key))
                {
                    tag.Metadata[kvp.Key] = kvp.Value;
                }
            }
        }

        return query.TagWith(tag.ToString());
    }

    /// <summary>
    /// Adds a tag to the query from the current tag scope.
    /// </summary>
    /// <typeparam name="T">The type of the query elements.</typeparam>
    /// <param name="query">The query to tag.</param>
    /// <returns>The tagged query, or the original query if no scope is active.</returns>
    public static IQueryable<T> TagWithScope<T>(this IQueryable<T> query)
    {
        var scopeTag = TagScope.Current;
        if (scopeTag == null)
        {
            return query;
        }

        return query.TagWith(scopeTag.Tag.ToString());
    }
}
