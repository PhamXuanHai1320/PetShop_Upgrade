using PetShop_Upgrade.DTOS.Foods.Admin;
using PetShop_Upgrade.DTOS.Foods.Client;
using PetShop_Upgrade.DTOS.Products.Client;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IFoodsDetailService
    {
        Task CreateFoodDetailAsync(int productId, CreateFoodDTO createFoodDTO);
        Task UpdateFoodDetailAsync(UpdateFoodDTO updateFoodDTO, int productId);
        Task<FoodResponseRequestDTO> GetFoodDetailByIdAsync(int productId);
        Task<IEnumerable<ProductItemsDTO>> GetFoodDetailByFillerAsync(FoodFilterDTO foodFilterDTO, int page, int pageSize);
        Task<IEnumerable<AdminFoodItemDTO>> AdminGetFoodDetailByFillerAsync(AdminFoodFilterDTO foodFilterDTO, int page, int pageSize);
    }
}
