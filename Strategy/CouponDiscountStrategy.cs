namespace ShopAPI.Strategy
{
    public class CouponDiscountStrategy : IDiscountStrategy
    {
        private readonly decimal _couponAmount; 
        private readonly string _couponCode;

        // Coupon details injected via constructor
        public CouponDiscountStrategy(string couponCode, decimal couponAmount)
        {
            _couponCode = couponCode;
            _couponAmount = couponAmount;
        }

        public string DiscountType => "Coupon";

        public decimal Calculate(decimal amount)
        {
            var discounted = amount - _couponAmount;
            return discounted < 0 ? 0 : discounted;
        }

        public string GetDescription(decimal original, decimal discounted)
        {
            var saved = original - discounted;
            return $"Coupon '{_couponCode}' applied - saved {saved:C}";
        }
    }
}  
