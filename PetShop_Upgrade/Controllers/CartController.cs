using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS.Cart;
using PetShop_Upgrade.Services.Interfaces;
using System.Security.Claims;

namespace PetShop_Upgrade.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int memberId => int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var result = await _cartService.GetCartAsync(memberId);
            return Ok(result);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequestDTO request)
        {
            var result = await _cartService.AddToCartAsync(memberId, request);
            return Ok(result);
        }

        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateCartItemRequestDTO request)
        {
            var result = await _cartService.UpdateCartItemAsync(memberId, cartItemId, request);
            return Ok(result);
        }

        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var result = await _cartService.RemoveCartItemAsync(memberId, cartItemId);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCartAsync(memberId);
            return NoContent();
        }
    }
}
