using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using InventarioApi;
using InventarioApi.Models;
using Xunit;

namespace InventarioApi.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(AppDbContext));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            // Use a unique in-memory SQLite database per test factory instance
            var dbName = Guid.NewGuid().ToString();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbName};Mode=Memory;Cache=Shared"));

            // Ensure the database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}

public class StockEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public StockEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostStock_NewItem_Returns201AndCreatesItem()
    {
        // Arrange
        var request = new
        {
            Nombre = "Laptop",
            Cantidad = 5,
            Ubicacion = "Deposito A",
            Categoria = "Electronica"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/items/stock", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var item = await response.Content.ReadFromJsonAsync<Item>();
        Assert.NotNull(item);
        Assert.Equal("Laptop", item!.Nombre);
        Assert.Equal(5, item.Cantidad);
        Assert.Equal("Deposito A", item.Ubicacion);
        Assert.Equal("Electronica", item.Categoria);
        Assert.True(item.Id > 0);
    }

    [Fact]
    public async Task PostStock_ExistingItemSameNombreAndUbicacion_Returns200AndSumsCantidad()
    {
        // Arrange — seed an item first
        var first = new
        {
            Nombre = "Monitor",
            Cantidad = 3,
            Ubicacion = "Sala B",
            Categoria = "Electronica"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/items/stock", first);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<Item>();
        Assert.NotNull(created);

        var second = new
        {
            Nombre = "Monitor",
            Cantidad = 7,
            Ubicacion = "Sala B",
            Categoria = "Electronica"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/items/stock", second);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Item>();
        Assert.NotNull(updated);
        Assert.Equal(created!.Id, updated!.Id);
        Assert.Equal(10, updated.Cantidad); // 3 + 7
    }
}
