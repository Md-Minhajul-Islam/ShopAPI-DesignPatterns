using ShopAPI.Models;
using ShopAPI.Strategy;

namespace ShopAPI.Factory
{
    public interface IDiscountStrategyFactory
    {
        IDiscountStrategy Create(DiscountType discountType, string couponCode = null);
    }
}