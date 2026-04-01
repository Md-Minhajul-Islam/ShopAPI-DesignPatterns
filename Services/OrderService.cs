using System.Diagnostics;
using ShopAPI.Exceptions;
using ShopAPI.Factory;
using ShopAPI.Models;
using ShopAPI.Repositories;
using ShopAPI.Singleton;
using ShopAPI.Strategy;

namespace ShopAPI.Services
{
    public class OrderService : IOrderService
    {

        private readonly IOrderRepository _orderRepo;
        private readonly IProductRepositories _productRepo;
        private readonly IPaymentProcessorFactory _paymentFactory;
        private readonly IDiscountStrategyFactory _discountFactory;
        private readonly IAppConfigService _config;
        public OrderService(
            IOrderRepository orderRepo, 
            IProductRepositories productRepo, 
            IPaymentProcessorFactory paymentFactory,
            IDiscountStrategyFactory discountFactory,
            IAppConfigService config
        )
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo; 
            _paymentFactory = paymentFactory;
            _discountFactory = discountFactory;
            _config = config;
        }



        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
            => await _orderRepo.GetAllAsync();

        public async Task<Order?> GetOrderByIdAsync(int id)
            => await  _orderRepo.GetByIdAsync(id);

        public async Task<Order> PlaceOrderAsync(
            int productId, 
            int quantity, 
            PaymentMethod paymentMethod,
            DiscountType discountType = DiscountType.None,
            string? couponCode = null
            )
        {
            var product = await _productRepo.GetByIdAsync(productId) 
                ?? throw new NotFoundException($"Product with ID {productId} not found");


            // NEW: use singleton config for validation
            if(!_config.IsOrderQuantityValid(quantity))
                throw new ValidationException(
                $"Quantity must be between 1 and " +
                $"{_config.Config.MaxOrderQuantity}");

            if(product.Stock < quantity)
                throw new ValidationException($"Not enough stock. Available: {product.Stock}, Requested: {quantity}");
            
            // NEW: low stock warning in response
            var remainingStock = product.Stock - quantity;
            if(_config.IsLowStock(remainingStock))
                Console.WriteLine($"⚠️ Low stock warning: " +
                              $"{product.Name} has {remainingStock} left!");

            var totalAmount = product.Price * quantity;

            // Strategy Pattern
            IDiscountStrategy strategy = _discountFactory.Create(discountType, couponCode);

            var context = new DiscountContext();

            context.SetStrategy(strategy); // inject strategy into context

            // apply the discount - context doesn't know which strategy
            var discountedAmount = context.ApplyDiscount(totalAmount);
            var discountNote = context.GetDiscountDescription(totalAmount, discountedAmount);


            var order = new Order
            {
                ProductId     = productId,
                Quantity      = quantity,
                TotalAmount   = totalAmount,
                DiscountedAmount = discountedAmount,
                DiscountNote = discountNote,
                DiscountType = discountType,
                PaymentMethod = paymentMethod,
                Status        = OrderStatus.Pending
            };

            // Factory Pattern
            IPaymentProcessor processor = _paymentFactory.Create(paymentMethod);

            bool paymentSuccess = await processor.ProcessAsync(order);

            if(!paymentSuccess) throw new ValidationException("Payment processing failed");

            product.Stock -= quantity;
            await _productRepo.UpdateAsync(product);

            await _orderRepo.AddAsync(order);

            return order;
        }
    }
}