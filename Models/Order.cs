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

    public enum DiscountType
    {
        None = 0,
        Seasonal = 1,
        Loyalty = 2,
        Coupon = 3
    }

    public class Order
    {
        public int Id {get; set;}
        public int ProductId {get; set;}
        public Product? Product {get; set;}  // Navigation property -> EF Core join
        public int Quantity {get; set;}
        public decimal TotalAmount {get; set;}
        public decimal DiscountedAmount {get; set;}
        public string? DiscountNote {get; set;}
        public PaymentMethod PaymentMethod {get; set;} // Factory reads this
        public DiscountType DiscountType {get; set;} = DiscountType.None;
        public OrderStatus Status {get; set;} = OrderStatus.Pending;
        public string? PaymentNote {get; set;} // Factory writes result here
        
        // NEW: Shipping fields
        public string? TrackingCode { get; set; }  
        public decimal ShippingCost { get; set; }        
        public string? ShippingStatus { get; set; }     
        public DateTime? EstimatedDelivery { get; set; } 
        
        
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

    }
}