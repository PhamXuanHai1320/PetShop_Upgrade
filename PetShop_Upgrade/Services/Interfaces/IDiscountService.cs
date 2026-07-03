using PetShop_Upgrade.DTOS.Discounts;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<DiscountDTO> GetDiscountByIdAsync(int id);
        Task<DiscountDTO> GetDiscountByCodeAsync(string code);
        Task<IEnumerable<DiscountDTO>> GetDiscountByFillerAsync(DiscountFilterDTO discountFilterDTO);
        Task<CreateDiscountDTO> CreateDiscountAsync(CreateDiscountDTO createDiscountDTO);
        Task<CreateDiscountDTO> UpdateDiscountAsync(int id, CreateDiscountDTO createDiscountDTO);
        Task DeleteDiscountAsync(int id);
    }
}
