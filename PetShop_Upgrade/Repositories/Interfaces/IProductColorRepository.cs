using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IProductColorRepository : IRepository<ProductColor>
    {
        Task<ProductColor?> GetForUpdateAsync(int productColorId);
        Task<List<ProductColor>> GetForUpdateBatchAsync(IEnumerable<int> productColorIds);
    }
}
