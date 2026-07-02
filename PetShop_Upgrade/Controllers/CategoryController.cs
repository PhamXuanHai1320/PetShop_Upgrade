using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS.Members.Client;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById([FromRoute] int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(category);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetCategoryByName(string name)
        {
            var categories = await _categoryService.GetCategoryByNameAsync(name);
            return Ok(categories);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody]CategoryDTO categoryDTO)
        {
            var createdCategory = await _categoryService.AddCategoryAsync(categoryDTO);
            return Ok(createdCategory);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] CategoryDTO categoryDTO)
        {
            categoryDTO.Id = id;
            var updatedCategory = await _categoryService.UpdateCategoryAsync(categoryDTO);
            return Ok(updatedCategory);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}
