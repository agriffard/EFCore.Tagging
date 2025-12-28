using EFCore.Tagging;
using EFCore.Tagging.Sample.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core with SQLite and SQL logging
builder.Services.AddDbContext<SampleDbContext>(options =>
{
    options.UseSqlite("Data Source=sample.db");
    
    // Enable detailed logging to see tagged SQL queries
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
});

// Add EF Core Tagging with configuration
builder.Services.AddEfCoreTagging(options =>
{
    options.Enabled = true;
    options.IncludeUser = true;
    options.IncludeEndpoint = true;
    options.IncludeCorrelationId = true;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure database is created with seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add EF Core Tagging middleware - should be before controllers
app.UseRouting();
app.UseAuthorization();

app.UseEfCoreTagging();

app.MapControllers();

app.Run();
