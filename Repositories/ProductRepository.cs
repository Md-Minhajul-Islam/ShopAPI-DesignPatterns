using Microsoft.EntityFrameworkCore;
using ShopAPI.Data;
using ShopAPI.Models;

namespace ShopAPI.Repositories;

public class ProductRepository : IProductRepositories
{
    private readonly AppDbContext _db;

    // AppDbContext is injected by .NET DI - we never write "new AppDbContext()"
    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    // READ - get all products from MS SQL
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _db.Products.AsNoTracking().ToListAsync();
    }

    // READ - get one product by id
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    // WRITE - add new product
    public async Task AddAsync(Product product)
    {
        await _db.Products.AddAsync(product);
        await _db.SaveChangesAsync(); // commits to MS SQL
    }

    // WRITE - update existing product
    public async Task UpdateAsync(Product product)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }

    // WRITE - delete product by id
    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if(product != null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
        }
    }

    // UTILITY - check if product exists
    public async Task<bool> ExistsAsync(int id)
    {
        return await _db.Products.AnyAsync(p => p.Id == id);
    }
}