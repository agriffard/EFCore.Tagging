namespace EFCore.Tagging.Tests;

public class TagScopeTests
{
    [Fact]
    public void Begin_CreatesNewScope()
    {
        // Arrange & Act
        using var scope = TagScope.Begin("Orders", "GetById");

        // Assert
        Assert.NotNull(TagScope.Current);
        Assert.Equal("Orders", TagScope.Current.Tag.Name);
        Assert.Equal("GetById", TagScope.Current.Tag.Metadata["Action"]);
    }

    [Fact]
    public void Dispose_RestoresParentScope()
    {
        // Arrange
        using var parentScope = TagScope.Begin("Parent");
        
        // Act
        using (var childScope = TagScope.Begin("Child"))
        {
            Assert.Equal("Child", TagScope.Current?.Tag.Name);
        }

        // Assert
        Assert.Equal("Parent", TagScope.Current?.Tag.Name);
    }

    [Fact]
    public void Dispose_RestoresNullWhenNoParent()
    {
        // Act
        using (var scope = TagScope.Begin("Test"))
        {
            Assert.NotNull(TagScope.Current);
        }

        // Assert
        Assert.Null(TagScope.Current);
    }

    [Fact]
    public void WithMetadata_AddsMetadataToScope()
    {
        // Arrange
        using var scope = TagScope.Begin("Test");

        // Act
        scope.WithMetadata("UserId", "123");

        // Assert
        Assert.Equal("123", TagScope.Current?.Tag.Metadata["UserId"]);
    }

    [Fact]
    public void WithMetadata_ReturnsScope_ForChaining()
    {
        // Arrange
        using var scope = TagScope.Begin("Test");

        // Act
        var result = scope.WithMetadata("Key1", "Value1")
                          .WithMetadata("Key2", "Value2");

        // Assert
        Assert.Same(scope, result);
        Assert.Equal("Value1", scope.Tag.Metadata["Key1"]);
        Assert.Equal("Value2", scope.Tag.Metadata["Key2"]);
    }

    [Fact]
    public void ChildScope_InheritsParentMetadata()
    {
        // Arrange
        using var parentScope = TagScope.Begin("Parent");
        parentScope.WithMetadata("CorrelationId", "abc123");

        // Act
        using var childScope = TagScope.Begin("Child");

        // Assert
        Assert.Equal("abc123", childScope.Tag.Metadata["CorrelationId"]);
    }

    [Fact]
    public void ChildScope_CanOverrideParentMetadata()
    {
        // Arrange
        using var parentScope = TagScope.Begin("Parent");
        parentScope.WithMetadata("Key", "ParentValue");

        // Act
        using var childScope = TagScope.Begin("Child");
        childScope.WithMetadata("Key", "ChildValue");

        // Assert
        Assert.Equal("ChildValue", childScope.Tag.Metadata["Key"]);
    }
}
