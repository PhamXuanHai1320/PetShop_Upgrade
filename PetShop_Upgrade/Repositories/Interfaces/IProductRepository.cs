using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<bool> HasProductsByCategoryIdAsync(int categoryId);
        Task<Product> GetProductByIdAsync(int productId);
        Task<Product> GetProductDetailsByIdAsync(int productId);
        Task<IEnumerable<Product>> GetProductByFilterAsync(ProductFilterDTO productFilter, int page, int pageSize);
    }
}
