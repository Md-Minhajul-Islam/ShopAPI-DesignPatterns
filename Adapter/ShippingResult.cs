namespace ShopAPI.Adapter
{
    public class ShippingResult
    {
        public bool Success {get; set;}
        public string TrackingCode {get; set;} = string.Empty;
        public string Message {get; set;} = string .Empty;
        public decimal ShippingCost {get; set;}
        public DateTime EstimateDelivery {get; set;}
    }
} 
