using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS.Products.Client;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductDetailById([FromRoute] int productId)
        {
            var productDetailDTO = await _productService.GetProductDetailByIdAsync(productId);
            return Ok(productDetailDTO);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProduct([FromQuery]ProductFilterDTO productFiller,[FromQuery] int page, [FromQuery] int pageSize)
        {
            var products = await _productService.GetProductByFilterAsync(productFiller, page, pageSize);
            return Ok(products);
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int productId)
        {
            await _productService.DeleteProductAsync(productId);
            return NoContent();
        }
    }
}
