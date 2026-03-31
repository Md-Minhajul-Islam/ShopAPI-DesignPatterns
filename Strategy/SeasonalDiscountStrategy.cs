namespace ShopAPI.Strategy
{
    public class SeasonalDiscountStrategy : IDiscountStrategy
    {
        private const decimal DiscountPercent = 0.10m; // 10% off

        public string DiscountType => "Seasonal";

        public decimal Calculate(decimal amount)
        {
            return amount-(amount*DiscountPercent);
        }

        public string GetDescription(decimal original, decimal discounted)
        {
            var saved = original-discounted;
            return $"Seasonal discount (10%) applied - saved {saved:C}";
        }
    }
}