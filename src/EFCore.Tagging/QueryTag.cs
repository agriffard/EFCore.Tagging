namespace EFCore.Tagging;

/// <summary>
/// Represents a tag that can be applied to an EF Core query.
/// </summary>
public class QueryTag
{
    /// <summary>
    /// Gets or sets the name of the tag.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata to include in the tag.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Converts the tag to a formatted string for use with TagWith().
    /// </summary>
    /// <returns>A formatted string representation of the tag.</returns>
    public override string ToString()
    {
        if (Metadata.Count == 0)
        {
            return Name;
        }

        var metadataString = string.Join(", ", Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{Name} [{metadataString}]";
    }
}
