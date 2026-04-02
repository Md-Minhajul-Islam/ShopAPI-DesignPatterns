namespace ShopAPI.Adapter
{
    public interface IShippingService
    {
        Task<ShippingResult> ShipOrderAsync(
            int productId,
            int quantity,
            string address
        );
        Task<decimal> GetShippingCostAsync(
            int productId,
            int quantity
        );
        Task<string> TrackShipmentAsync(string trackingCode);
    }
}