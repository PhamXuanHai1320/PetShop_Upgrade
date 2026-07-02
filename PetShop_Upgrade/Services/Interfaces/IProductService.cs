using PetShop_Upgrade.DTOS.Products.Admin;
using PetShop_Upgrade.DTOS.Products.Client;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IProductService
    {
        Task<int> CreateBaseProductAsync(CreateProductDTO dto, int type);
        Task UpdateBaseProductAsync(UpdateProductDTO updateProductDTO, int productId);
        Task<ProductDetailDTO> GetProductDetailByIdAsync(int id);
        Task<IEnumerable<ProductItemsDTO>> GetProductByFilterAsync(ProductFilterDTO productFilter, int page, int pageSize);
        Task DeleteProductAsync(int productId);
    }
}
