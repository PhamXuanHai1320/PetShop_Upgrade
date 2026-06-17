using PetShop_Upgrade.DTOS;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO> GetCategoryByIdAsync(int id);
        Task<IEnumerable<CategoryDTO>> GetCategoryByNameAsync(string Name);
        Task<CategoryDTO> AddCategoryAsync(CategoryDTO categoryDTO);
        Task<CategoryDTO> UpdateCategoryAsync(CategoryDTO categoryDTO);
        Task DeleteCategoryAsync(int id);
    }
}
