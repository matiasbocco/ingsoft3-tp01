using Microsoft.EntityFrameworkCore;
using InventarioApi.Models;

namespace InventarioApi;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();
}
