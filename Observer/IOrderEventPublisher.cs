namespace ShopAPI.Observer
{
    public interface IOrderEventPublisher
    {
        void Subscribe(IOrderEventObserver observer);
        void Unsubscribe(IOrderEventObserver observer);
        Task NotifyAsync(IOrderEvent orderEvent);
    }
}