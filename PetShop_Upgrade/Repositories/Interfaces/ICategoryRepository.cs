using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetCategoriesByNameAsync(string name);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
    }
}
