using ShopAPI.Models;

namespace ShopAPI.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order> PlaceOrderAsync(
            int productId, 
            int quantity, 
            PaymentMethod paymentMethod,
            DiscountType discountType = DiscountType.None,
            string? couponCode = null
            );
    }
}