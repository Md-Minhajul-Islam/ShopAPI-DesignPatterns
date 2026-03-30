using ShopAPI.Models;

namespace ShopAPI.Services;

public interface IProductService
{
    // READ
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);

    // WRITE
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(int id, Product product);
    Task DeleteProductAsync(int it);

    // BUSINESS LOGIC specific operations
    Task<IEnumerable<Product>> GetProductsInStockAsync();
    Task<bool> IsProductAvailableAsync(int id, int requestQuantity);
}