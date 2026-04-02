namespace ShopAPI.Adapter
{
    /// <summary>
    /// Legacy shipping system — simulates old third-party code.
    /// DO NOT MODIFY — treat as external dependency!
    /// </summary>
    public class LegacyShippingSystem
    {
        // Ships product — returns weird string format "SHIP_{id}_{qty}_{timestamp}"
        public string ship_product(int prod_id, int qty, string addr)
        {
            Console.WriteLine($"[LEGACY] Shipping {qty}x product#{prod_id} to {addr}");

            // Simulates legacy tracking code generation
            var timestamp = DateTime.UtcNow.Ticks;
            return $"SHIP_{prod_id}_{qty}_{timestamp}";
        }

        // Returns cost as string "COST:{amount}" — not decimal!
        public string get_shipping_cost(int prod_id, int qty)
        {
            Console.WriteLine($"[LEGACY] Getting cost for {qty}x product#{prod_id}");

            // Legacy flat rate calculation
            var cost = qty <= 5 ? 150 : 250;
            return $"COST:{cost}";
        }

        // Returns status as "STATUS:{value}" string
        public string track_shipment(string ship_code)
        {
            Console.WriteLine($"[LEGACY] Tracking shipment: {ship_code}");
            return "STATUS:IN_TRANSIT";
        }
    }
}