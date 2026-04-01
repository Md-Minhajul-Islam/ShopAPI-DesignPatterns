using System.Diagnostics;
using ShopAPI.Models;
using ShopAPI.Repositories;

namespace ShopAPI.Decorator
{
    public class LoggingProductRepository : IProductRepositories
    {
        private readonly IProductRepositories _inner; // Real repository
        private readonly ILogger<LoggingProductRepository> _logger;

        // Inject the Real repository + Logger
        public LoggingProductRepository(
            IProductRepositories inner,
            ILogger<LoggingProductRepository> logger
        )
        {
            _inner = inner;
            _logger = logger;
        }

        // GET all
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("[DB] Fetching all products...");

            try
            {
                var result = await _inner.GetAllAsync(); // Real repo call
                sw.Stop();

                _logger.LogInformation("[DB] Fetched {Count} products in {Ms}ms",
                result.Count(), sw.ElapsedMilliseconds);

                return result;
            }
            catch(Exception ex)
            {
                sw.Stop();
                _logger.LogError(
                    "[DB] GetAllAsync failed after {Ms}ms - {Error}",
                    sw.ElapsedMilliseconds, ex.Message
                );
                throw; // Re-throw - don't swallow the exception
            }
        }

        // GET by ID
        public async Task<Product?> GetByIdAsync(int id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("[DB] Fetching product ID: {Id}...", id);

            try
            {
                var result = await _inner.GetByIdAsync(id);
                sw.Stop();

                if (result == null)
                    _logger.LogWarning(
                        "[DB] Product ID: {Id} NOT FOUND ({Ms}ms)",
                        id, sw.ElapsedMilliseconds);
                else
                    _logger.LogInformation(
                        "[DB] Found product '{Name}' in {Ms}ms",
                        result.Name, sw.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(
                    "[DB] GetByIdAsync({Id}) failed after {Ms}ms — {Error}",
                    id, sw.ElapsedMilliseconds, ex.Message);
                throw;
            }
        }

        // Add
        public async Task AddAsync(Product product)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation(
                "[DB] Adding product '{Name}'...",
                product.Name
            );

            try
            {
                await _inner.AddAsync(product);
                sw.Stop();

                _logger.LogInformation(
                    "[DB] Product '{Name}' added with ID: {Id} in {Ms}ms",
                    product.Name, product.Id, sw.ElapsedMilliseconds
                );
            }
            catch(Exception ex)
            {
                sw.Stop();
                _logger.LogError(
                    "[DB] AddAsync failed after {Ms}ms - {Error}",
                    sw.ElapsedMilliseconds, ex.Message
                );
                throw;
            }
        }

        // UPDATE
        public async Task UpdateAsync(Product product)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation(
                "[DB] Updating product ID: {Id}...", product.Id);

            try
            {
                await _inner.UpdateAsync(product);
                sw.Stop();

                _logger.LogInformation(
                    "[DB] Product ID: {Id} updated in {Ms}ms",
                    product.Id, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(
                    "[DB] UpdateAsync({Id}) failed after {Ms}ms — {Error}",
                    product.Id, sw.ElapsedMilliseconds, ex.Message);
                throw;
            }
        }

        // ── DELETE
        public async Task DeleteAsync(int id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation(
                "[DB] Deleting product ID: {Id}...", id);

            try
            {
                await _inner.DeleteAsync(id);
                sw.Stop();

                _logger.LogInformation(
                    "[DB] Product ID: {Id} deleted in {Ms}ms",
                    id, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(
                    "[DB] DeleteAsync({Id}) failed after {Ms}ms — {Error}",
                    id, sw.ElapsedMilliseconds, ex.Message);
                throw;
            }
        }

        // EXISTS
        public async Task<bool> ExistsAsync(int id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation(
                "[DB] Checking if product ID: {Id} exists...", id);

            try
            {
                var exists = await _inner.ExistsAsync(id);
                sw.Stop();

                _logger.LogInformation(
                    "[DB] Product ID: {Id} exists: {Exists} ({Ms}ms)",
                    id, exists, sw.ElapsedMilliseconds);

                return exists;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(
                    "[DB] ExistsAsync({Id}) failed after {Ms}ms — {Error}",
                    id, sw.ElapsedMilliseconds, ex.Message);
                throw;
            }
        }
    }
} 
