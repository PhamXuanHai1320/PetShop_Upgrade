using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS.Foods.Admin;
using PetShop_Upgrade.DTOS.Foods.Client;
using PetShop_Upgrade.Orchestrators.Interfaces;
using PetShop_Upgrade.Services;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodController : ControllerBase
    {
        private readonly IFoodsDetailService _foodsDetailService;
        private readonly IProductOrchestrator _productOrchestrator;

        public FoodController(IFoodsDetailService foodsDetailService, IProductOrchestrator productOrchestrator)
        {
            _foodsDetailService = foodsDetailService;
            _productOrchestrator = productOrchestrator;
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> CreateFoodVariant([FromForm] CreateFoodDTO createfoodDTO)
        {
            await _productOrchestrator.CreateFoodAsync(createfoodDTO);
            return Ok();
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateFoodVariant([FromForm] UpdateFoodDTO updateFoodDTO, [FromRoute] int productId)
        {
            var memberId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            updateFoodDTO.MemberId = memberId;
            _productOrchestrator.UpdateFoodAsync(updateFoodDTO, productId);
            return Ok();
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetFoodVariantDetailById([FromRoute] int productId)
        {
            var foodDetailDTO = await _foodsDetailService.GetFoodDetailByIdAsync(productId);
            return Ok(foodDetailDTO);
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("admin")]
        public async Task<IActionResult> AdminGetAllFood([FromQuery] AdminFoodFilterDTO foodFilterDTO, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var foods = await _foodsDetailService.AdminGetFoodDetailByFillerAsync(foodFilterDTO, page, pageSize);
            return Ok(foods);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllFood([FromQuery] FoodFilterDTO foodFilterDTO, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var foods = await _foodsDetailService.GetFoodDetailByFillerAsync(foodFilterDTO, page, pageSize);
            return Ok(foods);
        }
    }
}
