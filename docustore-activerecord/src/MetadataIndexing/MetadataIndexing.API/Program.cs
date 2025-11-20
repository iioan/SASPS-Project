using MetadataIndexing.Infrastructure.Data;
using MetadataIndexing.Domain.Common;
using MetadataIndexing.Domain.Services;
using MetadataIndexing.Infrastructure.Services;
using MetadataIndexing.API.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<MetadataIndexingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MetadataIndexingDb")));

// Register services
builder.Services.AddScoped<IDocumentSearchService, DocumentSearchService>();

var app = builder.Build();

// Configure Active Record pattern
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    
    // Initialize ServiceLocator for Active Record pattern
    ServiceLocator.Initialize(serviceProvider);
    
    // Initialize DbContextProvider for Active Record pattern
    MetadataIndexingDbContextProvider.Initialize(() => serviceProvider.GetRequiredService<MetadataIndexingDbContext>());
    
    // Apply migrations automatically
    var dbContext = serviceProvider.GetRequiredService<MetadataIndexingDbContext>();
    dbContext.Database.Migrate();
}

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapSearchEndpoints();
app.MapControllers();

app.Run();