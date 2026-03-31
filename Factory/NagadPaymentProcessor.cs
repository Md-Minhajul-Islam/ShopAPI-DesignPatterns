using ShopAPI.Models;

namespace ShopAPI.Factory
{
    public class NagadPaymentProcessor : IPaymentProcessor
    {
        public string PaymentType => "Nagad";

        public Task<bool> ProcessAsync(Order order)
        {
            // Real app: call Nagad API here
            Console.WriteLine($"Processing NAGAD payment of {order.TotalAmount:C}");

            order.PaymentNote = $"Nagad payment of {order.TotalAmount:C} sent successfully";
            order.Status = OrderStatus.Confirmed;

            return Task.FromResult(true);
        }
    }
}
