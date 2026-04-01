namespace ShopAPI.Observer
{
    public interface IOrderEventObserver
    {
        string ObserverName {get;}
        Task OnOrderPlaced(IOrderEvent orderEvent);
    }
}