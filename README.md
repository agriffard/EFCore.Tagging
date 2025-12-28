# EFCore.Tagging

A simple and standardized API to automatically apply `TagWith` to EF Core queries for improved debugging, observability, and SQL performance analysis.

## Features

- **Standardized Query Tagging**: Apply consistent tags to all EF Core queries
- **Automatic Scope-Based Tagging**: Automatically add context to queries within a scope
- **ASP.NET Core Middleware**: Automatically tag queries with HTTP request context
- **Metadata Support**: Add custom metadata (user, feature, correlation ID, etc.)
- **Zero SQL Impact**: Tags appear as comments in SQL, no performance impact

## Installation

```bash
dotnet add package EFCore.Tagging
```

## Quick Start

### 1. Register Services

```csharp
using EFCore.Tagging;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core Tagging services
builder.Services.AddEfCoreTagging(options =>
{
    options.Enabled = true;
    options.IncludeUser = true;
    options.IncludeEndpoint = true;
    options.IncludeCorrelationId = true;
});
```

### 2. Add Middleware (for ASP.NET Core)

```csharp
var app = builder.Build();

// Add tagging middleware before controllers
app.UseRouting();
app.UseAuthorization();
app.UseEfCoreTagging();
app.MapControllers();
```

### 3. Tag Your Queries

#### Using TagWithContext (Explicit)

```csharp
var products = await _context.Products
    .Where(p => p.Category == "Electronics")
    .TagWithContext("Products", new { 
        Feature = "Catalog", 
        Action = "FilterByCategory",
        Category = category 
    })
    .ToListAsync();
```

#### Using TagWithScope (Automatic from Middleware)

```csharp
// Queries automatically inherit tags from the HTTP request scope
var products = await _context.Products
    .TagWithScope()
    .ToListAsync();
```

#### Using TagScope (Manual Scope)

```csharp
using (TagScope.Begin("Orders", "GetById"))
{
    TagScope.Current?.WithMetadata("OrderId", id.ToString());
    
    var order = await _context.Orders
        .TagWithScope()
        .FirstOrDefaultAsync(o => o.Id == id);
}
```

## SQL Output Example

When you execute a tagged query, the SQL will include comments like:

```sql
-- Products [Feature=Catalog, Action=FilterByCategory, Method=GET, Path=/api/products/category/Electronics, CorrelationId=abc12345]

SELECT "p"."Id", "p"."Name", "p"."Description", "p"."Price"
FROM "Products" AS "p"
WHERE "p"."Category" = @category
```

## Configuration

### EfTaggingOptions

| Option | Default | Description |
|--------|---------|-------------|
| `Enabled` | `true` | Enable/disable tagging globally |
| `IncludeUser` | `true` | Include authenticated user in tags |
| `IncludeEndpoint` | `true` | Include HTTP path in tags |
| `IncludeCorrelationId` | `true` | Include correlation ID in tags |
| `CorrelationIdHeader` | `"X-Correlation-ID"` | Header name for correlation ID |
| `AllowedMetadataKeys` | `[]` | Whitelist of allowed metadata keys (empty = all allowed) |

### appsettings.json

```json
{
  "EfTagging": {
    "Enabled": true,
    "IncludeUser": true,
    "IncludeEndpoint": true,
    "IncludeCorrelationId": true
  }
}
```

## API Reference

### QueryTag

```csharp
public class QueryTag
{
    public string Name { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}
```

### Extension Methods

```csharp
// Add a tag with name and optional metadata
query.TagWithContext("Products", new { Feature = "Catalog" });

// Add tag from current scope
query.TagWithScope();
```

### TagScope

```csharp
// Begin a new scope
using var scope = TagScope.Begin("Module", "Action");

// Add metadata
scope.WithMetadata("Key", "Value");

// Access current scope
var current = TagScope.Current;
```

## Use Cases

- **SQL Server Profiler**: Identify queries by feature/action
- **Production Debugging**: Correlate API requests with SQL queries
- **Performance Analysis**: Group slow queries by module
- **APM Integration**: Add context for observability tools

## Sample Application

See the `samples/EFCore.Tagging.Sample` project for a complete ASP.NET Core example with:
- Products and Orders API
- Automatic middleware tagging
- Manual scope-based tagging
- SQL logging with tags

## License

MIT