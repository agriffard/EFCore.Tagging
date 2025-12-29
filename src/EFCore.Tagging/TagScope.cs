namespace EFCore.Tagging;

/// <summary>
/// Represents a tag scope that automatically applies tags to queries executed within it.
/// </summary>
public class TagScope : IDisposable
{
    private static readonly AsyncLocal<TagScope?> _current = new();
    private readonly TagScope? _parent;
    private bool _disposed;

    /// <summary>
    /// Gets the current active tag scope.
    /// </summary>
    public static TagScope? Current => _current.Value;

    /// <summary>
    /// Gets the tag associated with this scope.
    /// </summary>
    public QueryTag Tag { get; }

    private TagScope(string name, string? action)
    {
        _parent = _current.Value;
        Tag = new QueryTag
        {
            Name = name,
            Metadata = new Dictionary<string, string>()
        };

        if (!string.IsNullOrEmpty(action))
        {
            Tag.Metadata["Action"] = action;
        }

        // Copy parent metadata if exists
        if (_parent != null)
        {
            foreach (var kvp in _parent.Tag.Metadata)
            {
                if (!Tag.Metadata.ContainsKey(kvp.Key))
                {
                    Tag.Metadata[kvp.Key] = kvp.Value;
                }
            }
        }

        _current.Value = this;
    }

    /// <summary>
    /// Begins a new tag scope with the specified name and optional action.
    /// </summary>
    /// <param name="name">The name of the scope (e.g., module or feature name).</param>
    /// <param name="action">Optional action name.</param>
    /// <returns>A new TagScope that should be disposed when the scope ends.</returns>
    public static TagScope Begin(string name, string? action = null)
    {
        return new TagScope(name, action);
    }

    /// <summary>
    /// Adds metadata to the current scope.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>This scope for method chaining.</returns>
    public TagScope WithMetadata(string key, string value)
    {
        Tag.Metadata[key] = value;
        return this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _current.Value = _parent;
            _disposed = true;
        }
    }
}
