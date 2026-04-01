namespace ShopAPI.Singleton
{
    public class AppConfiguration
    {
        public string AppName {get; set;} = "ShopAPI";
        public string AppVersion {get; set;} = "1.0.0";
        public int MaxOrderQuantity {get; set;} = 50;
        public decimal MaxProductPrice {get; set;} = 100000m;
        public decimal MinProductPrice {get; set;} = 1m;
        public decimal CouponDiscountAmount {get; set;} = 150m;
        public int LowStockThreshold {get; set;} = 5;
    }
} 
