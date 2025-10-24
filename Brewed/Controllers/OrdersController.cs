using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Brewed.DataContext.Dtos;
using Brewed.Services;
using System.Security.Claims;

namespace Brewed.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isAdmin = User.IsInRole("Admin");

                var order = await _orderService.GetOrderByIdAsync(orderId, userId, isAdmin);
                return Ok(order);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Order not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto orderDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _orderService.CreateOrderAsync(userId, orderDto);
                return CreatedAtAction(nameof(GetOrder), new { orderId = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _orderService.CancelOrderAsync(orderId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Order not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] string status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync(status, page, pageSize);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] OrderStatusUpdateDto dto)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(orderId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Order not found");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{orderId}/invoice")]
        public async Task<IActionResult> GetInvoice(int orderId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isAdmin = User.IsInRole("Admin");

                var invoice = await _orderService.GetInvoiceAsync(orderId, userId, isAdmin);
                return Ok(invoice);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Invoice not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{orderId}/invoice")]
        public async Task<IActionResult> GenerateInvoice(int orderId)
        {
            try
            {
                var invoice = await _orderService.GenerateInvoiceAsync(orderId);
                return Ok(invoice);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Order not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}