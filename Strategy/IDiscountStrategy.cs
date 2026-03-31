namespace ShopAPI.Strategy
{
    public interface IDiscountStrategy
    {
        string DiscountType{get;}
        decimal Calculate(decimal amount);
        string GetDescription(decimal originalAmount, decimal discountAmount);
    }
}
