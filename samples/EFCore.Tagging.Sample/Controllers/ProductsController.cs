using EFCore.Tagging.Sample.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Tagging.Sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly SampleDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(SampleDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all products with automatic tagging from middleware.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        _logger.LogInformation("Getting all products");
        
        // The middleware automatically tags queries within this scope
        var products = await _context.Products
            .TagWithScope()  // Uses the automatic scope from middleware
            .ToListAsync();
            
        return Ok(products);
    }

    /// <summary>
    /// Gets products by category using TagWithContext for explicit tagging.
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<List<Product>>> GetByCategory(string category)
    {
        _logger.LogInformation("Getting products by category: {Category}", category);
        
        // Manual tagging with context
        var products = await _context.Products
            .Where(p => p.Category == category)
            .TagWithContext("Products", new { Feature = "Catalog", Action = "FilterByCategory", Category = category })
            .ToListAsync();
            
        return Ok(products);
    }

    /// <summary>
    /// Gets a product by ID demonstrating scope-based tagging.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        _logger.LogInformation("Getting product by ID: {Id}", id);
        
        // Using scope-based tagging explicitly
        using (TagScope.Begin("Products", "GetById"))
        {
            TagScope.Current?.WithMetadata("ProductId", id.ToString());
            
            var product = await _context.Products
                .TagWithScope()
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (product == null)
            {
                return NotFound();
            }
            
            return Ok(product);
        }
    }

    /// <summary>
    /// Search products demonstrating combined tagging approaches.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<Product>>> Search([FromQuery] string? name, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
    {
        _logger.LogInformation("Searching products with name: {Name}, minPrice: {MinPrice}, maxPrice: {MaxPrice}", name, minPrice, maxPrice);
        
        var query = _context.Products.AsQueryable();
        
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(p => p.Name.Contains(name) || p.Description.Contains(name));
        }
        
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }
        
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }
        
        // Tag with search context
        var products = await query
            .TagWithContext("ProductSearch", new 
            { 
                Feature = "Catalog", 
                Action = "Search",
                HasNameFilter = !string.IsNullOrEmpty(name),
                HasPriceFilter = minPrice.HasValue || maxPrice.HasValue
            })
            .ToListAsync();
            
        return Ok(products);
    }
}
