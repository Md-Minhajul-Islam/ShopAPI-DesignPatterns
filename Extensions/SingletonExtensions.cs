using ShopAPI.Singleton;

namespace ShopAPI.Extensions
{
    public static class SingletonExtensions
    {
        public static IServiceCollection AddAppConfiguration(
            this IServiceCollection services
        )
        {
            services.AddSingleton<IAppConfigService, AppConfigService>();

            return services;
        }
    }
}