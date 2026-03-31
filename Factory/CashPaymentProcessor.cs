using ShopAPI.Models;

namespace ShopAPI.Factory
{
    public class CashPaymentProcessor : IPaymentProcessor
    {
        public string PaymentType => "Cash";
        public Task<bool> ProcessAsync(Order order)
        {
            // Real app: record cash payment, notify cashier etc
            // For now: simulate processin

            Console.WriteLine($"Processing CASH payment of {order.TotalAmount:C}");

            order.PaymentNote = $"Cash payment of {order.TotalAmount:C} collected at counter.";
            order.Status = OrderStatus.Confirmed;

            return Task.FromResult(true);
        }
    }
} 
