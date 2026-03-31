using Microsoft.AspNetCore.Mvc;
using ShopAPI.Exceptions;
using ShopAPI.Models;
using ShopAPI.Services;

namespace ShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _service.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _service.GetOrderByIdAsync(id);
            if (order == null) return NotFound($"Order {id} not found");
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            try
            {
                var order = await _service.PlaceOrderAsync(
                    request.ProductId,
                    request.Quantity,
                    request.PaymentMethod
                );

                return CreatedAtAction(nameof(GetById),
                new {id = order.Id}, order);
            }
            catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

// Request DTO - clean input model for the endpoint
public record PlaceOrderRequest(
    int ProductId,
    int Quantity,
    PaymentMethod PaymentMethod
);