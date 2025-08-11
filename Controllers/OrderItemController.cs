using API.Model;
using API.Model.DTO;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderItemController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] CreateOrderDTO orderDto)
        {
            try
            {
                var result = await _orderService.CreateOrderAsync(orderDto);
                if (result.Success)
                {
                    return Ok(new { success = true, orderNumber = result.OrderNumber });
                }
                else
                {
                    return BadRequest(new { message = result.OrderNumber });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDTO orderDto)
        {
            if (id != orderDto.Id)
                return BadRequest("Order ID mismatch.");

            try
            {
                var updated = await _orderService.UpdateOrderAsync(orderDto);
                if (!updated)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var deleted = await _orderService.DeleteOrderAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}