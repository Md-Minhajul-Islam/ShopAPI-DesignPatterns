using ShopAPI.Models;

namespace ShopAPI.Repositories;

public interface IProductRepositories
{
    // READ
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);

    // WRITE
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);

    // UTILITY
    Task<bool> ExistsAsync(int id); 
}
