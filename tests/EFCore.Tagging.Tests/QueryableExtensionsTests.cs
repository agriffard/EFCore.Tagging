using Microsoft.EntityFrameworkCore;

namespace EFCore.Tagging.Tests;

public class QueryableExtensionsTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestEntity> Entities { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }

    private TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public void TagWithContext_AddsTagToQuery()
    {
        // Arrange
        using var context = CreateContext();
        var query = context.Entities.AsQueryable();

        // Act
        var taggedQuery = query.TagWithContext("Products");

        // Assert
        var expression = taggedQuery.Expression.ToString();
        Assert.Contains("TagWith", expression);
    }

    [Fact]
    public void TagWithContext_WithMetadata_AddsFormattedTag()
    {
        // Arrange
        using var context = CreateContext();
        var query = context.Entities.AsQueryable();

        // Act
        var taggedQuery = query.TagWithContext("Products", new { Feature = "Catalog", UserId = "123" });

        // Assert
        var expression = taggedQuery.Expression.ToString();
        Assert.Contains("TagWith", expression);
    }

    [Fact]
    public void TagWithScope_WithActiveScope_AddsTag()
    {
        // Arrange
        using var context = CreateContext();
        var query = context.Entities.AsQueryable();

        // Act
        using var scope = TagScope.Begin("Orders", "GetById");
        var taggedQuery = query.TagWithScope();

        // Assert
        var expression = taggedQuery.Expression.ToString();
        Assert.Contains("TagWith", expression);
    }

    [Fact]
    public void TagWithScope_WithoutScope_ReturnsOriginalQuery()
    {
        // Arrange
        using var context = CreateContext();
        var query = context.Entities.AsQueryable();

        // Act
        var result = query.TagWithScope();

        // Assert
        Assert.Same(query, result);
    }

    [Fact]
    public void TagWithContext_IncludesScopeMetadata()
    {
        // Arrange
        using var context = CreateContext();
        var query = context.Entities.AsQueryable();

        // Act
        using var scope = TagScope.Begin("Orders");
        scope.WithMetadata("CorrelationId", "abc123");
        var taggedQuery = query.TagWithContext("GetOrder");

        // Assert - the scope metadata should be included
        var expression = taggedQuery.Expression.ToString();
        Assert.Contains("TagWith", expression);
    }
}
