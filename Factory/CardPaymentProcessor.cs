using ShopAPI.Models;

namespace ShopAPI.Factory
{
    public class CardPaymentProcessor : IPaymentProcessor
    {
        public string PaymentType => "Card";
        public Task<bool> ProcessAsync(Order order)
        {
            // Real app: call card payment gateway API here
            Console.WriteLine($"Processing CARD payment of {order.TotalAmount:C}");
            order.PaymentNote = $"Card payment of {order.TotalAmount:C} charges successfully";
            order.Status = OrderStatus.Confirmed;

            return Task.FromResult(true);
        }
    }
} 
