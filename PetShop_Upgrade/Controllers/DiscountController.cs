using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Claims;
using PetShop_Upgrade.DTOS.Discounts;
using PetShop_Upgrade.DTOS.Order;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        // Admin
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<DiscountDTO>>> GetDiscountsByFilter([FromQuery] DiscountFilterDTO filter)
        {
            var discounts = await _discountService.GetDiscountByFillerAsync(filter);
            return Ok(discounts);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("{id}")]
        public async Task<ActionResult<DiscountDTO>> GetDiscountById([FromRoute] int id)
        {
            var discount = await _discountService.GetDiscountByIdAsync(id);
            return Ok(discount);
        }

        [HttpGet("code")]
        public async Task<ActionResult<DiscountDTO>> GetDiscountByCode([FromQuery] string code)
        {
            var discount = await _discountService.GetDiscountByCodeAsync(code);
            return Ok(discount);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountDTO createDiscountDTO)
        {
            var createdBy = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            createDiscountDTO.CreatedBy = int.Parse(createdBy);
            var createdDiscount = await _discountService.CreateDiscountAsync(createDiscountDTO);
            return Ok(createdDiscount);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiscount([FromRoute] int id, [FromBody] CreateDiscountDTO updateDiscountDTO)
        {
            var createdBy = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            updateDiscountDTO.CreatedBy = int.Parse(createdBy);
            var updatedDiscount = await _discountService.UpdateDiscountAsync(id, updateDiscountDTO);
            return Ok(updateDiscountDTO);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscount([FromRoute] int id)
        {
            await _discountService.DeleteDiscountAsync(id);
            return NoContent();
        }
        [Authorize]
        [HttpPost("items")]
        public async Task<ActionResult<IEnumerable<DiscountItemsDTO>>> GetDiscountsByProductItems([FromBody] IEnumerable<CreateOrderItemRequestDTO> orderItems)
        {
            var discounts = await _discountService.GetDiscountsByProductItemsAsync(orderItems);
            return Ok(discounts);
        }
    }
}