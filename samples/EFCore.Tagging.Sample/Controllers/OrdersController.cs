using EFCore.Tagging.Sample.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Tagging.Sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly SampleDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(SampleDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all orders with related items - demonstrates tagging with Include.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAll()
    {
        _logger.LogInformation("Getting all orders");
        
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .TagWithContext("Orders", new { Feature = "OrderManagement", Action = "List" })
            .ToListAsync();
            
        return Ok(orders);
    }

    /// <summary>
    /// Gets an order by ID with nested scope tagging.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetById(int id)
    {
        _logger.LogInformation("Getting order by ID: {Id}", id);
        
        // Demonstrates nested scopes
        using var orderScope = TagScope.Begin("Orders", "GetById");
        orderScope.WithMetadata("OrderId", id.ToString());
        
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .TagWithScope()
            .FirstOrDefaultAsync(o => o.Id == id);
            
        if (order == null)
        {
            return NotFound();
        }
        
        return Ok(order);
    }

    /// <summary>
    /// Creates a new order - demonstrates tagging in write operations.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation("Creating new order for customer: {CustomerName}", request.CustomerName);
        
        using var scope = TagScope.Begin("Orders", "Create");
        scope.WithMetadata("CustomerName", request.CustomerName);
        
        // Validate products exist
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .TagWithScope()
            .ToDictionaryAsync(p => p.Id);
            
        if (products.Count != productIds.Count)
        {
            return BadRequest("One or more products not found");
        }
        
        var order = new Order
        {
            CustomerName = request.CustomerName,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = products[i.ProductId].Price
            }).ToList()
        };
        
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
