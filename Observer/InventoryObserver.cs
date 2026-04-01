namespace ShopAPI.Observer
{
    public class InventoryObserver : IOrderEventObserver
    {
        public string ObserverName => "Inventory System";

        public Task OnOrderPlaced(IOrderEvent orderEvent)
        {
            var order = orderEvent.Order;

            // Real app: sync with warehouse/ inventory system
            Console.WriteLine($"[INVENTORY] Stock update logged");
            Console.WriteLine($"    Product #{order.ProductId} — " +
                              $"{order.Quantity} unit(s) dispatched");
            Console.WriteLine($"    Remaining stock updated in warehouse ");

            return Task.CompletedTask;
        }
    }
}