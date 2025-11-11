using DocuStore.AR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();  
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (builder.Environment.IsDevelopment())
    {
        await dbContext.Database.MigrateAsync();
        
        // Optional: Seed data
        // await SeedData(dbContext);
    }
}

app.MapOpenApi();          // serves /openapi/v1.json (Microsoft.OpenApi pipeline)
app.UseSwagger();          // serves /swagger/v1/swagger.json (Swashbuckle pipeline)
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/openapi/v1.json", "DocuStore Active Record API v1");
    c.RoutePrefix = "active"; 
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}