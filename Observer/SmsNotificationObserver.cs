using ShopAPI.Observer;

public class SmsNotificationObserver : IOrderEventObserver
{
    public string ObserverName => "Sms Notification";

    public Task OnOrderPlaced(IOrderEvent orderEvent)
    {
        var order = orderEvent.Order;

        // Real app: call SMS geteway (Twillo, local SMS API etc)
        Console.WriteLine($"[SMS] Sending to customer...");
        Console.WriteLine($"    'Your order #{order.Id} has been placed! " +
                              $"Total: {order.DiscountedAmount:C}. Thank you!'");

        return Task.CompletedTask;
    }
}