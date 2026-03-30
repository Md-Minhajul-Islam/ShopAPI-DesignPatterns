using Microsoft.AspNetCore.Mvc;
using ShopAPI.Exceptions;
using ShopAPI.Models;
using ShopAPI.Services;

namespace ShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _service.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await _service.GetProductByIdAsync(id);
            return Ok(product);
        }
        catch(NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("instock")]
    public async Task<IActionResult> GetInStock()
    {
        var products = await _service.GetProductsInStockAsync();
        return Ok(products);
    }

    [HttpGet("{id}/available")]
    public async Task<IActionResult> CheckAvailability(int id, [FromQuery] int quantity)
    {
        var isAvailable = await _service.IsProductAvailableAsync(id, quantity);
        return Ok(new {productId = id,
        requestedQuantity = quantity,
        isAvailable});
    }


    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Product product)
    {
        try
        {
            await _service.AddProductAsync(product);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                product);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);       
        }
        catch (DuplicateException ex)
        {
            return Conflict(ex.Message);       
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        try
        {
            await _service.UpdateProductAsync(id, product);
            return NoContent();                 
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);          
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);     
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteProductAsync(id);
            return NoContent();                 
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);          
        }
    }
}