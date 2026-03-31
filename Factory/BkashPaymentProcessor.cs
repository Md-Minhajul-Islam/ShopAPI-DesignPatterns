 
using ShopAPI.Models;

namespace ShopAPI.Factory
{
    public class BkashPaymentProcessor : IPaymentProcessor
    {
        public string PaymentType => "bKash";

        public Task<bool> ProcessAsync(Order order)
        {
            // Real app: call bKash API here
            Console.WriteLine($"Processing BKASH payment of {order.TotalAmount:C}");

            order.PaymentNote = $"bKash payment of {order.TotalAmount:C} sent successfully";
            order.Status = OrderStatus.Confirmed;

            return Task.FromResult(true);
        }
    }
}