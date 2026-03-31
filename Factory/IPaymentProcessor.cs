using ShopAPI.Models;

namespace ShopAPI.Factory
{
    public interface IPaymentProcessor
    {
        string PaymentType {get;}
        Task<bool> ProcessAsync(Order order); // Returns true = success
    }
} 
