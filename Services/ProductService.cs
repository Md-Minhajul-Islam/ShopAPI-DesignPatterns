using ShopAPI.Exceptions;
using ShopAPI.Models;
using ShopAPI.Repositories;

namespace ShopAPI.Services;

public class ProductService : IProductService
{
    private readonly IProductRepositories _repo;

    public ProductService(IProductRepositories repo)
    {
        _repo = repo;
    }

    // READ OPERATIONS
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id);

        if(product == null)
        {
            throw new NotFoundException($"Product with ID {id} was not found.");
        }
        return product;
    }

    // BUSINESS LOGIC OPERATIONS
    public async Task<IEnumerable<Product>> GetProductsInStockAsync()
    {
        var allProducts = await _repo.GetAllAsync();

        return allProducts.Where(p => p.Stock > 0);
    }

    public async Task<bool> IsProductAvailableAsync(int id, int requestedQuantity)
    {
        var product = await _repo.GetByIdAsync(id);
        if(product == null) return false;
        return product.Stock >= requestedQuantity;
    }

    // WRITE OPERATIONS
    public async Task AddProductAsync(Product product)
    {
        ValidateProduct(product);

        var existing = await _repo.GetAllAsync();
        bool nameExists = existing.Any(p => p.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase));
        
        if(nameExists)
            throw new DuplicateException($"A Product named '{product.Name}' already exists.");

        product.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(product);
    }

    public async Task UpdateProductAsync(int id, Product product)
    {
        ValidateProduct(product);

        if(id != product.Id)
            throw new ValidationException("URL ID and product ID do not match.");

        await _repo.UpdateAsync(product);
    }

    public async Task DeleteProductAsync(int id)
    {
        if(id <= 0)
            throw new ValidationException("ID must be a positive number");

        await _repo.DeleteAsync(id);
    }




    // PRIVATE HELPERS
    private void ValidateProduct(Product product)
    {
        if(product == null)
            throw new ValidationException("Product cannot be null.");

        if(string.IsNullOrWhiteSpace(product.Name))
            throw new ValidationException("Product name is required.");
        
        if(product.Name.Length < 3)
            throw new ValidationException("Product name must be at least 3 characters.");
        
        if(product.Price <= 0)
            throw new ValidationException("Price must be greater than zero.");
        
        if(product.Stock < 0)
            throw new ValidationException("Stock cannot be negative.");
    }
}