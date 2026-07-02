using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS.Toys.Admin;
using PetShop_Upgrade.DTOS.Toys.Client;
using PetShop_Upgrade.Orchestrators.Interfaces;
using PetShop_Upgrade.Services;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToyController : ControllerBase
    {
        private readonly IToysDetailService _toysDetailService;
        private readonly IProductOrchestrator _productOrchestrator;

        public ToyController(IToysDetailService toysDetailService, IProductOrchestrator productOrchestrator)
        {
            _toysDetailService = toysDetailService;
            _productOrchestrator = productOrchestrator;
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> CreateToyVariant([FromForm] CreateToyDTO createtoyDTO)
        {
            await _productOrchestrator.CreateToyAsync(createtoyDTO);
            return Ok();
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateToyVariant([FromForm] UpdateToyDTO updateToyDTO, [FromRoute] int productId)
        {
            var memberId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            updateToyDTO.MemberId = memberId;
            await _productOrchestrator.UpdateToyAsync(updateToyDTO, productId);
            return Ok();
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("admin/{productId}")]
        public async Task<IActionResult> GetToyVariantDetailById([FromRoute] int productId)
        {
            var toyDetailDTO = await _toysDetailService.GetToyDetailByIdAsync(productId);
            return Ok(toyDetailDTO);
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("admin")]
        public async Task<IActionResult> AdminGetAllToy([FromQuery] AdminToyFilterDTO toyFilterDTO, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var toyDetailDTO = await _toysDetailService.AdminGetToyDetailByFillerAsync(toyFilterDTO, page, pageSize);
            return Ok(toyDetailDTO);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllToy([FromQuery] ToyFilterDTO toyFilterDTO, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pets = await _toysDetailService.GetToyDetailByFillerAsync(toyFilterDTO, page, pageSize);
            return Ok(pets);
        }
    }
}
