using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IProductHistoryRepository : IRepository<ProductHistory>
    {
        Task<int> GetLatestVersionAsync(int productId);
    }
}
