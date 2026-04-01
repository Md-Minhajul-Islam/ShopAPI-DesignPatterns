using ShopAPI.Observer;

namespace ShopAPI.Extensions
{
    public static class ObserverExtension
    {
        public static IServiceCollection AddObservers(
            this IServiceCollection services
        )
        {
            services.AddSingleton<IOrderEventPublisher, OrderEventPublisher>();
            services.AddSingleton<EmailNotificationObserver>();
            services.AddSingleton<SmsNotificationObserver>();
            services.AddSingleton<InventoryObserver>();
            services.AddSingleton<AnalyticsObserver>();

            return services;
        }

        public static WebApplication SubscribeObservers(
            this WebApplication app
        )
        {
            var publisher = app.Services.GetRequiredService<IOrderEventPublisher>();

            publisher.Subscribe(app.Services
                .GetRequiredService<EmailNotificationObserver>());
            publisher.Subscribe(app.Services
                .GetRequiredService<SmsNotificationObserver>());
            publisher.Subscribe(app.Services
                .GetRequiredService<InventoryObserver>());
            publisher.Subscribe(app.Services
                .GetRequiredService<AnalyticsObserver>());
            
            
            return app;
        }
    }
}