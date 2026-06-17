using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<int> GetCountProductsByCategoryIdAsync();
    }
}
