using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS.Order;
using PetShop_Upgrade.Orchestrators.Interfaces;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderOrchestration _orderOrchestration;
        private readonly IOrderService _orderService;

        public OrderController(IOrderOrchestration orderOrchestration, IOrderService orderService)
        {
            _orderOrchestration = orderOrchestration;
            _orderService = orderService;
        }

        private int memberId => int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);

        [Authorize(Roles = "Customer")]
        [HttpPost("preview")]
        public async Task<IActionResult> OrderPreview([FromBody] OrderPreviewRequestDTO dto)
        {
            var result = await _orderService.OrderPreview(dto);
            return Ok(result);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("from-cart")]
        public async Task<IActionResult> CreateOrderFromCart([FromBody] CreateOrderFromCartRequestDTO createOrderRequestDTO)
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            string ip = "127.0.0.1";

            if (remoteIp != null)
            {
                if (remoteIp.IsIPv4MappedToIPv6 || remoteIp.ToString() == "::1")
                {
                    ip = "127.0.0.1";
                }
                else
                {
                    ip = remoteIp.MapToIPv4().ToString();
                }
            }
            var orderId = await _orderOrchestration.CreateOrderFromCartAsync(memberId, createOrderRequestDTO, ip);
            return Ok(orderId);
        }
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderItemRequestDTO createOrderRequestDTO)
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            string ip = "127.0.0.1";

            if (remoteIp != null)
            {
                if (remoteIp.IsIPv4MappedToIPv6 || remoteIp.ToString() == "::1")
                {
                    ip = "127.0.0.1";
                }
                else
                {
                    ip = remoteIp.MapToIPv4().ToString();
                }
            }
            var orderId = await _orderOrchestration.CreateOrderAsync(memberId, createOrderRequestDTO, ip);
            return Ok(orderId);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] int id, [FromBody] UpdateOrderStatusRequestDTO updateOrderDTO)
        {
            await _orderService.UpdateOrderStatusAsync(id, memberId, updateOrderDTO);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrderByAdmin([FromRoute] int id, [FromBody] CancelOrderRequestDTO cancelOrderDTO)
        {
            await _orderService.CancelOrderByAdminAsync(id, memberId, cancelOrderDTO);
            return NoContent();
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("{id}/cancel/self")]
        public async Task<IActionResult> CancelOrderByMember([FromRoute] int id, [FromBody] CancelOrderRequestDTO cancelOrderDTO)
        {
            await _orderService.CancelOrderByMemberAsync(id, memberId, cancelOrderDTO);
            return NoContent();
        }
    }
}
