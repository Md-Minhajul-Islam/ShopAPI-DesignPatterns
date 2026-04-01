namespace ShopAPI.Observer
{
    public class OrderEventPublisher : IOrderEventPublisher
    {
        // List of all registered observers
        private readonly List<IOrderEventObserver> _observers = new();

        public void Subscribe(IOrderEventObserver observer)
        {
            _observers.Add(observer);
            Console.WriteLine($"[{observer.ObserverName}] subscribed to order events.");
        }

        public void Unsubscribe(IOrderEventObserver observer)
        {
            _observers.Remove(observer);
            Console.WriteLine($"[{observer.ObserverName}] unsubscribed from order events.");
        }

        public async Task NotifyAsync(IOrderEvent orderEvent)
        {
            Console.WriteLine($"Notifying {_observers.Count} observer(s)...");

            foreach(var observer in _observers)
            {
                try
                {
                    // Each observer handles its own logic
                    // Publisher doesn't care what they do
                    await observer.OnOrderPlaced(orderEvent);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"[{observer.ObserverName}] failed: {ex.Message}");
                }
            }
        }
    }
}