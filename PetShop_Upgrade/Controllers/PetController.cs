using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Orchestrators.Interfaces;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetController : ControllerBase
    {
        private readonly IPetVariantService _petVariantService;
        private readonly IProductOrchestrator _productOrchestrator;

        public PetController(IPetVariantService petVariantService, IProductOrchestrator productOrchestrator)
        {
            _petVariantService = petVariantService;
            _productOrchestrator = productOrchestrator;
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> CreatePetVariant([FromForm] CreatePetDTO createPetDTO)
        {
            await _productOrchestrator.CreatePetAsync(createPetDTO);
            return Ok();
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdatePetVariant([FromForm] UpdatePetDTO updatePetDTO, [FromRoute] int productId)
        {
            var memberId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            updatePetDTO.MemberId = memberId;
            await _productOrchestrator.UpdatePetAsync(updatePetDTO, productId);
            return Ok();
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("admin/{productId}")]
        public async Task<IActionResult> GetPetVariantDetailById([FromRoute] int productId)
        {
            var petDetailDTO = await _petVariantService.GetPetVariantByIdAsync(productId);
            return Ok(petDetailDTO);
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("admin")]
        public async Task<IActionResult> AdminGetAllPet([FromQuery] AdminPetFillerDTO petFilterDTO, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var petVariants = await _petVariantService.AdminGetPetVariantByFillerAsync(petFilterDTO, page, pageSize);
            return Ok(petVariants);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPet([FromQuery] PetFillerDTO petFilterDTO, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pets = await _petVariantService.GetPetVariantByFillerAsync(petFilterDTO, page, pageSize);
            return Ok(pets);
        }
    }
}
