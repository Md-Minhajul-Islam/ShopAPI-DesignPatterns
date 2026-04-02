using Microsoft.AspNetCore.Mvc;
using ShopAPI.Adapter;
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
        private readonly IShippingService _shipping;

        public OrdersController(
            IOrderService service,
            IShippingService shipping
        )
        {
            _service = service;
            _shipping = shipping;
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
                    request.PaymentMethod,
                    request.DiscountType,
                    request.CouponCode
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

        [HttpGet("{trackingCode}/track")]
        public async Task<IActionResult> TrackShipment(string trackingCode)
        {
            var status = await _shipping.TrackShipmentAsync(trackingCode);
            return Ok(new {trackingCode, status});
        }
    }
}

// Request DTO - clean input model for the endpoint
public record PlaceOrderRequest(
    int ProductId,
    int Quantity,
    PaymentMethod PaymentMethod,
    DiscountType DiscountType = DiscountType.None,
    string? CouponCode = null
);