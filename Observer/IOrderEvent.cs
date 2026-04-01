using ShopAPI.Models;

namespace ShopAPI.Observer
{
    public enum OrderEventType
    {
        OrderPlaced = 1,
        OrderCancelled = 2,
        OrderConfirmed = 3
    }

    public interface IOrderEvent
    {
        Order Order {get;}
        OrderEventType  EventType{get;}
        DateTime OccurredAt {get;}
    }

    // Concrete event - passed  to all observers
    public class OrderPlacedEvent : IOrderEvent
    {
        public Order Order {get;}
        public OrderEventType EventType => OrderEventType.OrderPlaced;
        public DateTime OccurredAt {get;} =  DateTime.UtcNow;

        public OrderPlacedEvent(Order order)
        {
            Order  = order;
        }
    }
}