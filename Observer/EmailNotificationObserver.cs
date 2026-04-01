namespace ShopAPI.Observer
{
    public class EmailNotificationObserver : IOrderEventObserver
    {
        public string ObserverName => "Email Notification";

        public Task OnOrderPlaced(IOrderEvent orderEvent)
        {
            var order = orderEvent.Order;

            // Real app: call Email service (SendGrid, SMTP etc)
            // For now: simulate sending email
            Console.WriteLine($"[EMAIL] Order #{order.Id} confirmed!");
            Console.WriteLine($"    To: customer@email.com");
            Console.WriteLine($"    Subject: Your order of {order.Quantity} " +
                              $"item(s) — Total: {order.DiscountedAmount:C}");

            return Task.CompletedTask;
        }
    }
}