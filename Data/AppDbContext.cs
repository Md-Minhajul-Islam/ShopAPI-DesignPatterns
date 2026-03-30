using Microsoft.EntityFrameworkCore;
using ShopAPI.Models;

namespace ShopAPI.Data;

public class AppDbContext : DbContext
{
    // Constructor - receives options (connection string) via DI
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    // Each DbSet = one table in MS SQL
    public DbSet<Product> Products {get; set;}

    // Optional: Fine-tue table/column config here later
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure Price has 2 decimal places in SQL
        modelBuilder.Entity<Product>()
        .Property(p => p.Price)
        .HasColumnType("decimal(18, 2)");

        // Name is required and max 200 characters
        modelBuilder.Entity<Product>()
        .Property(p => p.Name)
        .IsRequired()
        .HasMaxLength(200);
    }
}
