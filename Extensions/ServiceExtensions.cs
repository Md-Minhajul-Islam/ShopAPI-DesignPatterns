using ShopAPI.Services;

namespace ShopAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(
            this IServiceCollection services
        )
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
        
            return services;
        }
    }
}