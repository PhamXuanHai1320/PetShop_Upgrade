using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS;
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
            try
            {
                var colors = await _colorService.GetAllColorsAsync();
                return Ok(colors);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var color = await _colorService.GetColorByIdAsync(id);
                if (color == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = $"Color with id {id} not found"
                    });
                }
                return Ok(color);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Post([FromBody] ColorDTO colorDTO)
        {
            try
            {
                var createdColor = await _colorService.CreateColorAsync(colorDTO);
                return Ok(createdColor);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ColorDTO colorDTO)
        {
            try
            {
                var updatedColor = await _colorService.UpdateColorAsync(id, colorDTO);
                return Ok(updatedColor);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                await _colorService.DeleteColorAsync(id);
                return Ok(new
                {
                    Success = true,
                    Message = $"Màu có ID {id} đã được xóa thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
