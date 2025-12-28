namespace EFCore.Tagging.Tests;

public class EfTaggingOptionsTests
{
    [Fact]
    public void DefaultOptions_HaveCorrectValues()
    {
        // Arrange & Act
        var options = new EfTaggingOptions();

        // Assert
        Assert.True(options.Enabled);
        Assert.True(options.IncludeUser);
        Assert.True(options.IncludeEndpoint);
        Assert.True(options.IncludeCorrelationId);
        Assert.Equal("X-Correlation-ID", options.CorrelationIdHeader);
        Assert.Empty(options.AllowedMetadataKeys);
    }

    [Fact]
    public void Options_CanBeConfigured()
    {
        // Arrange
        var options = new EfTaggingOptions
        {
            Enabled = false,
            IncludeUser = false,
            IncludeEndpoint = false,
            IncludeCorrelationId = false,
            CorrelationIdHeader = "Custom-Correlation-ID",
            AllowedMetadataKeys = new List<string> { "Feature", "Module" }
        };

        // Assert
        Assert.False(options.Enabled);
        Assert.False(options.IncludeUser);
        Assert.False(options.IncludeEndpoint);
        Assert.False(options.IncludeCorrelationId);
        Assert.Equal("Custom-Correlation-ID", options.CorrelationIdHeader);
        Assert.Equal(2, options.AllowedMetadataKeys.Count);
    }
}
