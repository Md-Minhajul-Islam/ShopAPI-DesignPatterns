namespace ShopAPI.Strategy
{
    public class DiscountContext
    {
        // Current strategy - can be swapped at runtime
        private IDiscountStrategy _strategy;

        // Default to no discount
        public DiscountContext()
        {
            _strategy = new NoDiscountStrategy();
        }

        // Swap strategy from outside
        public void SetStrategy(IDiscountStrategy strategy)
        {   
            _strategy = strategy;
        }

        public decimal ApplyDiscount(decimal amount)
        {
            return _strategy.Calculate(amount);
        }

        public string GetDiscountDescription(decimal original, decimal discounted)
        {
            return _strategy.GetDescription(original, discounted);
        }

        public string CurrentStrategy => _strategy.DiscountType;
    }
}