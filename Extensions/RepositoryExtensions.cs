using ShopAPI.Decorator;
using ShopAPI.Repositories;

namespace ShopAPI.Extensions
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddRepositories(
            this IServiceCollection services
        )
        {
            // Product Repository with Decorator
            // Step 1: Register the REAL repository with a name
            //         - Concrete class - not the interface
            services.AddScoped<ProductRepository>();

            // Step 2: Register the INTERFACE → Decorator
            // When IProductRepository is requested → give LoggingProductRepository
            // LoggingProductRepository wraps ProductRepository inside
            services.AddScoped<IProductRepositories>(provider => 
                new LoggingProductRepository(provider.GetRequiredService<ProductRepository>(),
                provider.GetRequiredService<ILogger<LoggingProductRepository>>()    
            ));       
            
            // Order Repository without Decorator
            services.AddScoped<IOrderRepository, OrderRepository>();

            return services;
        }
    }
}