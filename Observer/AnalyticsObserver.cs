namespace ShopAPI.Observer
{
    public class AnalyticsObserver : IOrderEventObserver
    {
        public string ObserverName => "Analytics";

        public Task OnOrderPlaced(IOrderEvent orderEvent)
        {
            var order = orderEvent.Order;

            // Real app: send to analytics platform (Google Analytics, Mixpanel etc)
            Console.WriteLine($"    [ANALYTICS] Event logged");
            Console.WriteLine($"    Event: order_placed");
            Console.WriteLine($"    ProductId: {order.ProductId}");
            Console.WriteLine($"    Revenue: {order.DiscountedAmount:C}");
            Console.WriteLine($"    PaymentMethod: {order.PaymentMethod}");
            Console.WriteLine($"    DiscountType: {order.DiscountType}");

            return Task.CompletedTask;
        }
    }
}