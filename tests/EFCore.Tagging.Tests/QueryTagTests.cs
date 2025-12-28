namespace EFCore.Tagging.Tests;

public class QueryTagTests
{
    [Fact]
    public void ToString_WithNameOnly_ReturnsName()
    {
        // Arrange
        var tag = new QueryTag { Name = "Products" };

        // Act
        var result = tag.ToString();

        // Assert
        Assert.Equal("Products", result);
    }

    [Fact]
    public void ToString_WithMetadata_ReturnsFormattedString()
    {
        // Arrange
        var tag = new QueryTag
        {
            Name = "Products",
            Metadata = new Dictionary<string, string>
            {
                ["Feature"] = "Catalog",
                ["Action"] = "List"
            }
        };

        // Act
        var result = tag.ToString();

        // Assert
        Assert.StartsWith("Products [", result);
        Assert.Contains("Feature=Catalog", result);
        Assert.Contains("Action=List", result);
    }

    [Fact]
    public void ToString_WithEmptyMetadata_ReturnsNameOnly()
    {
        // Arrange
        var tag = new QueryTag
        {
            Name = "Orders",
            Metadata = new Dictionary<string, string>()
        };

        // Act
        var result = tag.ToString();

        // Assert
        Assert.Equal("Orders", result);
    }
}
