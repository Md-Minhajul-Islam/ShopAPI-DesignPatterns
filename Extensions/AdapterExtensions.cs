using ShopAPI.Adapter;

namespace ShopAPI.Extensions
{
    public static class AdapterExtensions
    {
        public static IServiceCollection AddAdapters(
            this IServiceCollection services
        )
        {
            services.AddScoped<IShippingService, LegacyShippingAdapter>();

            return services;
        }
    }
}