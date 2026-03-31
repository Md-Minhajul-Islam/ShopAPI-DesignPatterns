using System.Diagnostics;
using ShopAPI.Exceptions;
using ShopAPI.Factory;
using ShopAPI.Models;
using ShopAPI.Repositories;

namespace ShopAPI.Services
{
    public class OrderService : IOrderService
    {

        private readonly IOrderRepository _orderRepo;
        private readonly IProductRepositories _productRepo;
        private readonly IPaymentProcessorFactory _factory;

        public OrderService(IOrderRepository orderRepo, IProductRepositories productRepo, IPaymentProcessorFactory factory)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo; 
            _factory = factory;
        }



        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
            => await _orderRepo.GetAllAsync();

        public async Task<Order?> GetOrderByIdAsync(int id)
            => await  _orderRepo.GetByIdAsync(id);

        public async Task<Order> PlaceOrderAsync(int productId, int quantity, PaymentMethod paymentMethod)
        {
            var product = await _productRepo.GetByIdAsync(productId) ?? throw new NotFoundException($"Product with ID {productId} not found");

            if(product.Stock < quantity)
                throw new ValidationException($"Not enough stock. Available: {product.Stock}, Requested: {quantity}");
            
            var totalAmount = product.Price * quantity;

            var order = new Order
            {
                ProductId     = productId,
                Quantity      = quantity,
                TotalAmount   = totalAmount,
                PaymentMethod = paymentMethod,
                Status        = OrderStatus.Pending
            };

            IPaymentProcessor processor = _factory.Create(paymentMethod);

            bool paymentSuccess = await processor.ProcessAsync(order);

            product.Stock -= quantity;
            await _productRepo.UpdateAsync(product);

            await _orderRepo.AddAsync(order);

            return order;
        }
    }
}