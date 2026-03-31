using ShopAPI.Models;
using ShopAPI.Strategy;

namespace ShopAPI.Factory
{
    public class DiscountStrategyFactory : IDiscountStrategyFactory{
        public IDiscountStrategy Create(DiscountType discountType, string? couponCode = null)
        {
            return discountType switch
            {
                DiscountType.Seasonal => new SeasonalDiscountStrategy(),
                DiscountType.Loyalty => new LoyaltyDiscountStrategy(),
                DiscountType.Coupon => new CouponDiscountStrategy(couponCode ?? "DEFAULT", 150m),
                _ => new NoDiscountStrategy()
            };
        }
    } 
}