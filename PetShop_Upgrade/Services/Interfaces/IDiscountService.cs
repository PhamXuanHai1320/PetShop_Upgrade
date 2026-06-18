using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<IEnumerable<DiscountDTO>> GetAllDiscountsAsync();
        Task<DiscountDTO> GetDiscountByIdAsync(int id);
        Task<IEnumerable<DiscountDTO>> GetDiscountByNameAsync(string discountName);
        Task<DiscountDTO> GetDiscountByCodeAsync(string code);
        Task<IEnumerable<DiscountDTO>> GetDiscountByFillerAsync(DiscountFilterDTO discountFilterDTO);
        Task<IEnumerable<DiscountDTO>> GetDiscountsByProductIdAsync(int productId);
        Task<IEnumerable<DiscountDTO>> GetDiscountsByCategoryIdAsync(int categoryId);
        Task<CreateDiscountDTO> CreateDiscountAsync(CreateDiscountDTO createDiscountDTO);
        Task<CreateDiscountDTO> UpdateDiscountAsync(int id, CreateDiscountDTO createDiscountDTO);
        Task DeleteDiscountAsync(int id);
    }
}
