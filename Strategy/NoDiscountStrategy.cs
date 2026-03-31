namespace ShopAPI.Strategy
{
    public class NoDiscountStrategy : IDiscountStrategy
    {
        public string DiscountType => "None";

        public decimal Calculate(decimal amount)
        {
            return amount;
        }

        public string GetDescription(decimal original, decimal discounted)
        {
            return "No discount applied.";
        }
    }
} 
