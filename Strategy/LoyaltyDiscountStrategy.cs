namespace ShopAPI.Strategy
{
    public class LoyaltyDiscountStrategy : IDiscountStrategy
    {
        private const decimal DiscountPercent = 0.15m; //15% off
        
        public string DiscountType => "Loyalty";
        
        public decimal Calculate(decimal amount)
        {
            return amount-(amount * DiscountPercent);
        }

        public string GetDescription(decimal original, decimal discounted)
        {
            var saved = original-discounted;
            return $"Loyalty discount (15%) applied - saved {saved:C}";
        }
    }
}
