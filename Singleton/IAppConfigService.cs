namespace ShopAPI.Singleton
{
    public interface IAppConfigService
    {
        AppConfiguration Config {get;}

        // Helper methods
        AppConfiguration GetConfiguration();
        bool IsOrderQuantityValid(int quantity);
        bool IsProductPriceValid(decimal price);
        bool IsLowStock(int stock);
        decimal GetCouponDiscountAmount();
    }
} 
