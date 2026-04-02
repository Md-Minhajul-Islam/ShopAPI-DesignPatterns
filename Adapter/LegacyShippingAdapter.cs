namespace ShopAPI.Adapter
{
    public class LegacyShippingAdapter : IShippingService
    {
        // wraps the legacy system - just like Decorator wraps repository
        private readonly LegacyShippingSystem _legacy;

        public LegacyShippingAdapter()
        {
            _legacy = new LegacyShippingSystem();
        }

        public Task<ShippingResult> ShipOrderAsync(
            int productId,
            int quantity,
            string address
        )
        {
            try
            {
                // 1. Translate inputs + call legacy
                var legacyResponse = _legacy.ship_product(
                    prod_id: productId,
                    qty: quantity,
                    addr: address
                );

                // 2. Translate output - parse "SHIP_1_2_123456" => clean object
                var trackingCode = ParseTrackingCode(legacyResponse);

                // 3. Return modern clean reslt
                var result = new ShippingResult
                {
                    Success = true,
                    TrackingCode = trackingCode,
                    Message = "Order Shipped successfully",
                    EstimateDelivery = DateTime.UtcNow.AddDays(3)
                };

                return Task.FromResult(result);
            }
            catch(Exception ex)
            {
                // Translate legacy exceptions -> clean failure result
                return Task.FromResult(new ShippingResult
                {
                    Success = false,
                    Message = $"Shipping failed: {ex.Message}."
                });
            }
        }

        public Task<decimal> GetShippingCostAsync(
            int productId,
            int quantity
        )
        {
            // 1. Call legacy
            var legacyResponse = _legacy.get_shipping_cost(
                prod_id: productId,
                qty: quantity
            );

            // 2. Translate output - "COST:150" -> decimal 150
            var cost = ParseCost(legacyResponse);

            // 3. Return clean decimal
            return Task.FromResult(cost);
        }

        public Task<string> TrackShipmentAsync(string trackingCode)
        {
            // 1. Call legacy
            var legacyResponse = _legacy.track_shipment(trackingCode);

            // 2. Translate output — parse "STATUS:IN_TRANSIT" → "In Transit"
            var status = ParseStatus(legacyResponse);

            // 3. Return clean string 
            return Task.FromResult(status);
        }




        // PRIVATE TRANSLATORS
        // These are the heart of the adapter - parsing ugly legacy responses
        private string ParseTrackingCode(string legacyResponse)
        {
            // "SHIP_1_2_123" → "TRK-1_2_123"
            return legacyResponse.Replace("SHIP_", "TRK-");
        }

        private decimal ParseCost(string legacyResponse)
        {
            var parts = legacyResponse.Split(':');
            return parts.Length == 2
                ? decimal.Parse(parts[1]) : 0m;
        }

        private string ParseStatus(string legacyResponse)
        {
            // "STATUS:IN_TRANSIT" → "In Transit"
            var parts = legacyResponse.Split(':');
            if(parts.Length != 2) return "Unknown";

            return parts[1] switch
            {
                "IN_TRANSIT"  => "In Transit",
                "DELIVERED"   => "Delivered",
                "PENDING"     => "Pending",
                "RETURNED"    => "Returned",
                _             => "Unknown"
            };
        }
    }
}