using ShopAPI.Models;
using ShopAPI.Singleton;
using ShopAPI.Strategy;

namespace ShopAPI.Factory
{
    public class DiscountStrategyFactory : IDiscountStrategyFactory{
       
       private readonly IAppConfigService _config;

       public DiscountStrategyFactory(IAppConfigService config)
        {
            _config = config;
        }
       
        public IDiscountStrategy Create(DiscountType discountType, string? couponCode = null)
        {
            return discountType switch
            {
                DiscountType.Seasonal => new SeasonalDiscountStrategy(),
                DiscountType.Loyalty => new LoyaltyDiscountStrategy(),
                DiscountType.Coupon => new CouponDiscountStrategy(couponCode ?? "DEFAULT", _config.GetCouponDiscountAmount()),
                _ => new NoDiscountStrategy()
            };
        }
    } 
}