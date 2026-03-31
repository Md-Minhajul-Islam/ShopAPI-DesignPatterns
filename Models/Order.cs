namespace ShopAPI.Models
{
    public enum PaymentMethod
    {
        Cash = 1,
        Card = 2,
        Bkash = 3,
        Nagad = 4
    }

    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3
    }

    public class Order
    {
        public int Id {get; set;}
        public int ProductId {get; set;}
        public Product? Product {get; set;}  // Navigation property -> EF Core join
        public int Quantity {get; set;}
        public decimal TotalAmount {get; set;}
        public PaymentMethod PaymentMethod {get; set;} // Factory reads this
        public OrderStatus Status {get; set;} = OrderStatus.Pending;
        public string? PaymentNote {get; set;} // Factory writes result here
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

    }
}