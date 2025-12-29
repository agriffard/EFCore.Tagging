namespace EFCore.Tagging;

/// <summary>
/// Configuration options for EF Core Tagging.
/// </summary>
public class EfTaggingOptions
{
    /// <summary>
    /// Gets or sets whether tagging is enabled. Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include user information in tags. Default is true.
    /// </summary>
    public bool IncludeUser { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include endpoint information in tags. Default is true.
    /// </summary>
    public bool IncludeEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include correlation ID in tags. Default is true.
    /// </summary>
    public bool IncludeCorrelationId { get; set; } = true;

    /// <summary>
    /// Gets or sets the name of the correlation ID header. Default is "X-Correlation-ID".
    /// </summary>
    public string CorrelationIdHeader { get; set; } = "X-Correlation-ID";

    /// <summary>
    /// Gets or sets a list of metadata keys that are allowed in tags.
    /// If empty, all keys are allowed.
    /// </summary>
    public List<string> AllowedMetadataKeys { get; set; } = new();
}
