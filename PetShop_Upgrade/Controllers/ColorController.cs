using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS.Colors;
using PetShop_Upgrade.Services.Interfaces;
using System.Threading.Tasks;

namespace PetShop_Upgrade.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        private readonly IColorService _colorService;
        public ColorController(IColorService colorService)
        {
            _colorService = colorService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var colors = await _colorService.GetAllColorsAsync();
            return Ok(colors);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var color = await _colorService.GetColorByIdAsync(id);
            return Ok(color);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Post([FromBody] ColorDTO colorDTO)
        {
            var createdColor = await _colorService.CreateColorAsync(colorDTO);
            return Ok(createdColor);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ColorDTO colorDTO)
        {
            var updatedColor = await _colorService.UpdateColorAsync(id, colorDTO);
            return Ok(updatedColor);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _colorService.DeleteColorAsync(id);
            return Ok(new
            {
                Success = true,
                Message = $"Màu có ID {id} đã được xóa thành công"
            });
        }
    }
}
