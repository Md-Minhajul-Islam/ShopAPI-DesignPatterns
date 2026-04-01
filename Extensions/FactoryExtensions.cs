using ShopAPI.Factory;

namespace ShopAPI.Extensions
{
    public static class FactoryExtensions
    {
        public static IServiceCollection AddFactories(
            this IServiceCollection services
        )
        {
            services.AddSingleton<IPaymentProcessorFactory, PaymentProcessorFactory>();
            services.AddSingleton<IDiscountStrategyFactory, DiscountStrategyFactory>();

            return services;
        }
    }
}