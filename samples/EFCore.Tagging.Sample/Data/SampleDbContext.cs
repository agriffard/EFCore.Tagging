using Microsoft.EntityFrameworkCore;

namespace EFCore.Tagging.Sample.Data;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 1299.99m, Category = "Electronics", StockQuantity = 50, CreatedAt = DateTime.UtcNow },
            new Product { Id = 2, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Price = 49.99m, Category = "Electronics", StockQuantity = 200, CreatedAt = DateTime.UtcNow },
            new Product { Id = 3, Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard", Price = 129.99m, Category = "Electronics", StockQuantity = 75, CreatedAt = DateTime.UtcNow },
            new Product { Id = 4, Name = "Monitor 27\"", Description = "4K IPS monitor", Price = 449.99m, Category = "Electronics", StockQuantity = 30, CreatedAt = DateTime.UtcNow },
            new Product { Id = 5, Name = "USB-C Hub", Description = "Multi-port USB-C hub", Price = 79.99m, Category = "Accessories", StockQuantity = 100, CreatedAt = DateTime.UtcNow }
        );
    }
}
