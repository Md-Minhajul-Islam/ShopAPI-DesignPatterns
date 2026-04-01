namespace ShopAPI.Singleton
{
    public class AppConfigService : IAppConfigService
    {
        // // WAY 1:  Classic Singleton
        // private static AppConfigService? _instance;
        // private static readonly object _lock = new object();

        // public static AppConfigService Instance
        // {
        //     get
        //     {
        //         if(_instance == null)
        //         {
        //             // Thread safe - prevents two threads
        //             // creating two instances simultaneously
        //             lock (_lock)
        //             {
        //                 _instance ??= new AppConfigService();
        //             }
        //         }
        //         return _instance;
        //     }
        // }

        // Configuration
        public AppConfiguration Config {get; private set;}

        public AppConfigService()
        {
            // In real app: read from appsettings.json or environment
            // For ShopAPI: hardcoded defaults

            Config = new AppConfiguration
            {
                AppName              = "ShopAPI",
                AppVersion           = "1.0.0",
                MaxOrderQuantity     = 50,
                MaxProductPrice      = 100000m,
                MinProductPrice      = 1m,
                CouponDiscountAmount = 150m,
                LowStockThreshold    = 5
            };

            Console.WriteLine($"AppConfigService created - " + $"App: {Config.AppName} v{Config.AppVersion}");
        }

        
        // Helper methods
        public AppConfiguration GetConfiguration()
        {
            return Config;
        }
        public bool IsOrderQuantityValid(int quantity)
        {
            return quantity > 0 && quantity <= Config.MaxOrderQuantity;
        }

        public bool IsProductPriceValid(decimal price)
        {
            return price >= Config.MinProductPrice && price <= Config.MaxProductPrice;
        }

        public bool IsLowStock(int stock)
        {
            return stock <= Config.LowStockThreshold;
        }

        public decimal GetCouponDiscountAmount()
        {
            return Config.CouponDiscountAmount;
        }
    }
}