using ShopAPI.Repositories;

namespace ShopAPI.Extensions
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddRepositories(
            this IServiceCollection services
        )
        {
            services.AddScoped<IProductRepositories, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            return services;
        }
    }
}